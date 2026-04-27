# Palpite Amigo (backend)

### Sobre

Este projeto implementa uma API para organização de palpites esportivos.

Usuários cadastrados podem criar disputas onde visitantes palpitam para uma competição em particular, provendo regras customizadas de participação e pontuação.

### Objetos de Domínio

**Usuários**: Usuários da aplicação possuem e-mail, senha e apelido.

- Esquemas de autenticação e autorização são baseados no AspNet Core Identity.
- O sistema utiliza política de *roles*: *staff* e *adm*.
- Endpoints e objetos são protegidos onde necessário.

**Formatos**: Cada formato de competição define um modelo de dados esportivos e regras de pontuação a ser reforçado pela aplicação nas competições associadas.

**Competições**: Cada competição implementa a identidade de um torneio esportivo que faz uso de um formato conhecido.

**Jogos**: Usuários cadastrados podem criar e gerir jogos, que organizam palpites associados a uma competição selecionada. Seu autor pode definir regras de pontuação e acesso.

**Palpites**: Um palpite para resultados de eventos reais é atrelado a um jogo. Usuários da API podem palpitar sem autenticação, mas ainda sujeitos às regras de pontuação e acesso definidas pelo autor do jogo.

### Infra

Todo o projeto foi construído com o AspNet Core num esquema de arquitetura limpa com 5 componentes (DLLs).

**Api**: apresentação;

- O Swagger está disponível como opção para exploração da API pelo navegador.

**Applications**: casos de uso;

**Globals**: objetos compartilhados;

**Infrastructure**: bancos de dados e serviços externos;

- São utilizados o SQL Server e o SQLite, este último para testes.
- Há um serviço de envio de e-mails na forma de um cliente RabbitMQ que envia mensagens ao *message broker* com dados que outro serviço externo poderá consumir.

**Models**: Domínio.

### Execução

Um arquivo *appsettings.json* é esperado na pasta do projeto com as *strings* de conexão a bancos de dados.

```cmd
dotnet run [options]
```

As opções implementadas pelo *app* são as seguintes:

- -\-dbserver: Utiliza o servidor de banco de dados programado. Por padrão, o SQLite é utilizado.
- -\-msgserver: Utiliza o servidor de mensageria programado. Por padrão, um *stub*, que nada realiza, é utilizado.
- -\-swagger: Habilita a interação com a API pelo Swagger UI no navegador.

O *workflow* de teste executa a aplicação sem nenhuma opção.

### Demonstração

Acesse [este artigo](https://sites.google.com/view/fsouza/projetos/palpites-app) para aprender mais e ver uma demonstração do projeto com o frontend.
