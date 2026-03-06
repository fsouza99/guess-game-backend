# Palpite Amigo (backend)

## O popular *bolão*

### Sobre

Este projeto ASP<span>.</span>NET Core implementa uma Web API para validação, registro e valoração de competições esportivas e palpites para seus resultados.

Usuários cadastrados podem organizar disputas onde palpites podem ser deixados para uma competição em particular, provendo regras customizadas de participação e pontuação.

### Controladores principais

**Formatos**: Cada formato de competição define modelos de dados esportivos e regras de pontuação a serem reforçados pela aplicação.

**Competições**: Cada competição implementa a identidade de um torneio esportivo real que faz uso de um formato conhecido.

**Jogos**: Usuários cadastrados podem criar e gerir jogos, que organizam palpites direcionados a uma competição selecionada. Seu autor pode definir regras de pontuação e acesso.

**Palpites**: Um palpite para resultados de eventos reais é atrelado a um jogo. Usuários da API podem palpitar sem autenticação, mas ainda sujeitos às regras de pontuação e acesso definidas pelo autor do jogo.

### Execução

Um arquivo *appsettings.json* é esperado na pasta do projeto com as *strings* de conexão a bancos de dados.

```cmd
dotnet run [options]
```

As opções implementadas pelo *app* são as seguintes:

- --dbserver: Utiliza o servidor de banco de dados programado. Por padrão, o SQLite é utilizado.
- --msgserver: Utiliza o servidor de mensageria programado. Por padrão, um *stub*, que nada realiza, é utilizado.
- --swagger: Habilita a interação com a API pelo Swagger UI no navegador.

O *workflow* de teste executa a aplicação sem nenhuma opção.

### *Notas*

A aplicação valida os dados inseridos para garantir sua integridade.

Esquemas de autenticação e autorização são baseados no ASP<span>.</span>NET Core Identity.
