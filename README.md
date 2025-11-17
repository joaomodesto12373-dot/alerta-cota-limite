# Alerta de cota limite

## Objetivo do Projeto

O projeto consiste em uma aplicação de console desenvolvida em C# (.NET) cujo propósito é o **monitoramento contínuo da cotação de um ativo financeiro** e o **disparo de alertas por e-mail** quando o preço atinge limites pré-definidos de compra ou venda.

## Instruções de Uso

Para execução desta aplicação, o usuário deve seguir os passos descritos abaixo, garantindo que o ambiente de execução esteja devidamente configurado.

### 1. Pré-requisitos

Para compilar e executar o projeto, é necessário possuir:

- **SDK do .NET** (versão compatível com o projeto).

- **Git** (para clonagem do repositório).

- **Newtonsoft.Json** (pacote NuGet para manipulação de JSON).

### 2. Configuração do Arquivo `appsettings.json`

A aplicação carrega todas as configurações, incluindo as credenciais de e-mail, **diretamente do arquivo ****`appsettings.json`**.

**IMPORTANTE:** O arquivo `appsettings.json` deve ser criado **dentro da pasta do projeto** (`alertacotalimite/`).

A estrutura do arquivo deve ser a seguinte:

#### Exemplo de Estrutura Correta:

```json
{
  "Email": "cliente.email@destino.com",
  "Smtp": "smtp.meuprovedor.com",
  "Port": 587,
  "Username": "meu.email@autenticacao.com",
  "Password": "minha_senha_ou_app_password"
}
```

| Propriedade | Função | Exemplo de Valor |
| --- | --- | --- |
| `Email` | E-mail de destino para o recebimento dos alertas. | `cliente.email@destino.com` |
| `Smtp` | Servidor SMTP para envio de e-mails. | `smtp.meuprovedor.com` |
| `Port` | Porta do servidor SMTP. | `587` |
| `Username` | Usuário de autenticação do SMTP. | `meu.email@autenticacao.com` |
| `Password` | Senha de autenticação do SMTP (recomenda-se o uso de senha de app). | `minha_senha_ou_app_password` |

### 3. Execução da Aplicação

A aplicação é executada via linha de comando, aceitando 3 ou 4 argumentos.

**A sintaxe fundamental é:**

```bash
dotnet run <SIMBOLO> <PRECO_VENDA> <PRECO_COMPRA> [reset]
```

#### Exemplo de Uso (Monitoramento Padrão)

Para monitorar o ativo `PETR4` com um limite de venda em `32.70` e um limite de compra em `32.50`:

```bash
dotnet run PETR4 32.70 32.50
```

**Veja que** a ordem dos preços de venda e compra não é estritamente necessária, pois automaticamente identificamos o maior valor como o limite de venda e o menor como o limite de compra.

#### Exemplo de Uso (Reset de Memória)

Para iniciar o monitoramento e, antes de tudo, **apagar o histórico de alertas** (garantindo que um novo alerta seja enviado imediatamente se a condição for atendida):

```bash
dotnet run PETR4 32.70 32.50 reset
```

**OBS: Execute o comando dotnet run na pasta alertacotalimite.**

---

## Processo de Desenvolvimento 

O desenvolvimento deste projeto foi conduzido sob o princípio da **Separação de Responsabilidades**, resultando em uma arquitetura modular e de fácil manutenção. Commo imagino que se de o trabalho da Inoa.

### 1. Estrutura e Componentes

**Veja que** a aplicação foi dividida em componentes especializados, cada um com uma função bem definida:

| Componente | Classe(s) | Responsabilidade Primária |
| --- | --- | --- |
| **Ponto de Entrada** | `Program.cs` | Valida argumentos, carrega configurações, inicializa todos os serviços e o `AlertMonitor`. |
| **Configuração** | `Config.cs`, `ConfigService.cs` | Define a estrutura de dados para as configurações e carrega as credenciais de e-mail a partir das variáveis de ambiente. |
| **Cotação** | `QuoteService.cs` | Realiza requisições HTTP assíncronas à API `Brapi` para obter o preço atual do ativo. |
| **Alerta** | `EmailService.cs` | Conecta-se ao servidor SMTP (com SSL e autenticação) para formatar e enviar e-mails de alerta. |
| **Monitoramento** | `AlertMonitor.cs` | Orquestra o *loop* contínuo de monitoramento, chama o `QuoteService`, aplica a lógica de limites e chama o `EmailService`. |
| **Persistência** | `AlertState.cs`, `FileAlertPersistenceService.cs` | **Melhoria Implementada:** Armazena o estado (`BuyAlertSent`, `SellAlertSent`) em um arquivo JSON (`alertState.json`) para evitar o envio de alertas duplicados após reinicializações do programa. |

### 2. Fluxo de Execução e Lógica de Alerta

**Chegamos** então ao fluxo de execução, que é iniciado pelo `Program.cs` e orquestrado pelo `AlertMonitor`:

1. **Inicialização:** O `Program.cs` carrega as configurações e inicializa o `AlertMonitor`, injetando os serviços (`QuoteService`, `EmailService`, `FileAlertPersistenceService`) e os parâmetros de monitoramento.

1. **Carregamento do Estado:** O `AlertMonitor` **carrega o estado de persistência** (`AlertState`) do disco.

1. **Loop Contínuo:** O `AlertMonitor` entra em um *loop* infinito, onde a cada intervalo de tempo (30s):
  - O `QuoteService` é chamado para obter o preço atual.
  - O preço é comparado com os limites de compra e venda.
  - Note que o alerta só é disparado se a condição for atendida **E** o respectivo *flag* no `AlertState` estiver como `false`.

1. **Atualização do Estado:** Após a verificação, o `AlertMonitor` atualiza o `AlertState` (marcando o alerta como enviado) e o `FileAlertPersistenceService` **salva** o novo estado no disco.

1. **Reversão de Tendência:** A lógica de alerta inclui um mecanismo de *reset* interno: se um alerta de compra foi enviado e o preço se afasta do limite de compra (subindo), o *flag* `BuyAlertSent` é zerado, permitindo que um novo alerta seja enviado em um ciclo futuro.

**Assim, demonstramos** que a aplicação não apenas atende aos requisitos básicos de monitoramento, mas também incorpora mecanismos de resiliência e usabilidade essenciais para um sistema de monitoramento financeiro.

### 3. Melhorias  e Usabilidade 

1. **Robustez da API (****`QuoteService.cs`****):**
  - O serviço de cotação utiliza um bloco `try-catch` para lidar com falhas de rede ou erros da API. Em caso de falha, ele registra um aviso no console e **retorna um valor neutro (****`0`****)**, garantindo que o *loop* de monitoramento não seja interrompido.

1. **Lógica de Novo Alerta por Limite mais Agressivo:**
  - A persistência foi criada para armazenar o último preço que disparou o alerta.
  - Se o usuário iniciar o monitoramento com um limite **mais agressivo** do que o limite anterior (ex: Venda em R$33.00, e o limite anterior era R$32.50), a aplicação **reseta o *****flag***** de alerta** para permitir um novo disparo, mesmo que o alerta já tenha sido enviado anteriormente.

1. **Informações de Inicialização (****`AlertMonitor.cs`****):**
  - Ao iniciar, a aplicação exibe o **intervalo de consulta** e, se o arquivo de persistência existir, o **último preço salvo** e o **status atual dos alertas** (se já foram enviados ou não).




