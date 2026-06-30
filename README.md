# Postgraduate Tech Challenge

## 👥 Team Members

- Daniel - RM372640
- Domingos Pereira - RM373016
- Igor de Andrade Tudisco - RM371517
- Guilherme Paschoal - RM373362
- Pedro Svicero - RM370700

## 🛠️ Gerenciamento de Dados (Entity Framework Core)
Estes comandos devem ser executados via terminal (preferencialmente dentro da pasta CloudGame.API) para gerenciar o ciclo de vida das migrações no banco de dados Oracle.

1. Adicionar uma Migração
```Bash
dotnet ef migrations add NomeDaMigracao -p ..\CloudGame.Infrastructure
```
Descrição: Analisa as mudanças nas classes de entidade e gera o código C# necessário para atualizar o esquema do banco de dados.

Parâmetro -p: Aponta para o projeto onde o contexto de dados (DbContext) e as configurações de infraestrutura residem.

2. Remover a Última Migração
```Bash
dotnet ef migrations remove -p ..\CloudGame.Infrastructure
```
Descrição: Exclui os arquivos da última migração gerada que ainda não foi aplicada ao banco de dados.

Atenção: Se a migração já tiver sido enviada ao banco via update, você deve reverter o banco para a versão anterior antes de removê-la.

3. Atualizar o Banco de Dados
```Bash
dotnet ef database update --project ..\CloudGame.Infrastructure
```
Descrição: Executa os scripts SQL gerados pelas migrações pendentes, sincronizando o esquema do banco de dados Oracle com o modelo do código.

Uso: Essencial para aplicar as tabelas e restrições configuradas na camada de Infrastructure.

*IMPORTANT*

Dica de Fluxo: Se você cometer um erro ao criar uma migração, a sequência correta é:

database update (para uma versão anterior, se necessário).

migrations remove (para apagar os arquivos gerados erroneamente).

migrations add (com a correção).



## License
This project is licensed under the MIT License.
