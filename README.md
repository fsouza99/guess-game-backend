## O popular *bolão*

### Sobre

Este projeto ASP<span>.</span>NET Core implementa uma Web API para validação, registro e valoração de competições esportivas e palpites para seus resultados.

Usuários cadastrados podem organizar disputas onde palpites podem ser deixados para uma competição em particular, provendo regras customizadas de participação e pontuação.

### Controladores principais

**Formatos**: Cada formato de competição define modelos de inserção de dados esportivos e regras de pontuação a serem reforçados pela aplicação.

**Competições**: Cada competição implementa a identidade de um torneio esportivo real que faz uso de um formato salvo no banco de dados.

**Jogos**: Usuários cadastrados podem criar e gerir jogos, que colecionam palpites direcionados a uma competição específica. Seu autor pode definir também regras de acesso para demais visitantes do site.

**Palpites**: Um palpite para resultados de eventos reais é atrelado a um jogo. Usuários da API podem deixar um palpite sem a necessidade de autenticação, mas ainda sujeitos às regras de pontuação e acesso definidas pelo autor do jogo.

#### *Notas*

A aplicação valida os dados inseridos para garantir sua integridade.

Esquemas de autenticação e autorização são baseados no ASP<span>.</span>NET Core Identity.
