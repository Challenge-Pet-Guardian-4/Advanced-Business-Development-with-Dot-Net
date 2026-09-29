using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetGuardian.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncOracleUpperCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aula_modulo_modulo_id_modulo",
                table: "aula");

            migrationBuilder.DropForeignKey(
                name: "FK_bairro_cidade_cidade_id_cidade",
                table: "bairro");

            migrationBuilder.DropForeignKey(
                name: "FK_cidade_estado_estado_id_estado",
                table: "cidade");

            migrationBuilder.DropForeignKey(
                name: "FK_endereco_bairro_bairro_id_bairro",
                table: "endereco");

            migrationBuilder.DropForeignKey(
                name: "FK_historico_pet_pet_id_pet",
                table: "historico");

            migrationBuilder.DropForeignKey(
                name: "FK_modulo_trilha_trilha_id_trilha",
                table: "modulo");

            migrationBuilder.DropForeignKey(
                name: "FK_pet_raca_raca_id_raca",
                table: "pet");

            migrationBuilder.DropForeignKey(
                name: "FK_tarefa_pet_pet_id_pet",
                table: "tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_tarefa_status_status_id_status",
                table: "tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_tarefa_usuario_usuario_id_usuario",
                table: "tarefa");

            migrationBuilder.DropForeignKey(
                name: "FK_trilha_pet_pet_id_pet",
                table: "trilha");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_telefone_telefone_id_telefone",
                table: "usuario");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_endereco_endereco_endereco_id_endereco",
                table: "usuario_endereco");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_endereco_usuario_usuario_id_usuario",
                table: "usuario_endereco");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_pet_pet_pet_id_pet",
                table: "usuario_pet");

            migrationBuilder.DropForeignKey(
                name: "FK_usuario_pet_usuario_usuario_id_usuario",
                table: "usuario_pet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuario_pet",
                table: "usuario_pet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuario_endereco",
                table: "usuario_endereco");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuario",
                table: "usuario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_trilha",
                table: "trilha");

            migrationBuilder.DropPrimaryKey(
                name: "PK_telefone",
                table: "telefone");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tarefa",
                table: "tarefa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_status",
                table: "status");

            migrationBuilder.DropPrimaryKey(
                name: "PK_raca",
                table: "raca");

            migrationBuilder.DropPrimaryKey(
                name: "PK_pet",
                table: "pet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_modulo",
                table: "modulo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_historico",
                table: "historico");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estado",
                table: "estado");

            migrationBuilder.DropPrimaryKey(
                name: "PK_endereco",
                table: "endereco");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cidade",
                table: "cidade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bairro",
                table: "bairro");

            migrationBuilder.DropPrimaryKey(
                name: "PK_aula",
                table: "aula");

            migrationBuilder.RenameTable(
                name: "usuario_pet",
                newName: "USUARIO_PET");

            migrationBuilder.RenameTable(
                name: "usuario_endereco",
                newName: "USUARIO_ENDERECO");

            migrationBuilder.RenameTable(
                name: "usuario",
                newName: "USUARIO");

            migrationBuilder.RenameTable(
                name: "trilha",
                newName: "TRILHA");

            migrationBuilder.RenameTable(
                name: "telefone",
                newName: "TELEFONE");

            migrationBuilder.RenameTable(
                name: "tarefa",
                newName: "TAREFA");

            migrationBuilder.RenameTable(
                name: "status",
                newName: "STATUS");

            migrationBuilder.RenameTable(
                name: "raca",
                newName: "RACA");

            migrationBuilder.RenameTable(
                name: "pet",
                newName: "PET");

            migrationBuilder.RenameTable(
                name: "modulo",
                newName: "MODULO");

            migrationBuilder.RenameTable(
                name: "historico",
                newName: "HISTORICO");

            migrationBuilder.RenameTable(
                name: "estado",
                newName: "ESTADO");

            migrationBuilder.RenameTable(
                name: "endereco",
                newName: "ENDERECO");

            migrationBuilder.RenameTable(
                name: "cidade",
                newName: "CIDADE");

            migrationBuilder.RenameTable(
                name: "bairro",
                newName: "BAIRRO");

            migrationBuilder.RenameTable(
                name: "aula",
                newName: "AULA");

            migrationBuilder.RenameColumn(
                name: "respon_princ",
                table: "USUARIO_PET",
                newName: "RESPON_PRINC");

            migrationBuilder.RenameColumn(
                name: "pet_id_pet",
                table: "USUARIO_PET",
                newName: "PET_ID_PET");

            migrationBuilder.RenameColumn(
                name: "usuario_id_usuario",
                table: "USUARIO_PET",
                newName: "USUARIO_ID_USUARIO");

            migrationBuilder.RenameIndex(
                name: "IX_usuario_pet_pet_id_pet",
                table: "USUARIO_PET",
                newName: "IX_USUARIO_PET_PET_ID_PET");

            migrationBuilder.RenameColumn(
                name: "endereco_id_endereco",
                table: "USUARIO_ENDERECO",
                newName: "ENDERECO_ID_ENDERECO");

            migrationBuilder.RenameColumn(
                name: "usuario_id_usuario",
                table: "USUARIO_ENDERECO",
                newName: "USUARIO_ID_USUARIO");

            migrationBuilder.RenameIndex(
                name: "IX_usuario_endereco_endereco_id_endereco",
                table: "USUARIO_ENDERECO",
                newName: "IX_USUARIO_ENDERECO_ENDERECO_ID_ENDERECO");

            migrationBuilder.RenameColumn(
                name: "telefone_id_telefone",
                table: "USUARIO",
                newName: "TELEFONE_ID_TELEFONE");

            migrationBuilder.RenameColumn(
                name: "senha",
                table: "USUARIO",
                newName: "SENHA");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "USUARIO",
                newName: "ROLE");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "USUARIO",
                newName: "NOME");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "USUARIO",
                newName: "EMAIL");

            migrationBuilder.RenameColumn(
                name: "id_usuario",
                table: "USUARIO",
                newName: "ID_USUARIO");

            migrationBuilder.RenameIndex(
                name: "IX_usuario_telefone_id_telefone",
                table: "USUARIO",
                newName: "IX_USUARIO_TELEFONE_ID_TELEFONE");

            migrationBuilder.RenameIndex(
                name: "IX_usuario_email",
                table: "USUARIO",
                newName: "IX_USUARIO_EMAIL");

            migrationBuilder.RenameColumn(
                name: "pet_id_pet",
                table: "TRILHA",
                newName: "PET_ID_PET");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "TRILHA",
                newName: "NOME");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "TRILHA",
                newName: "DESCRICAO");

            migrationBuilder.RenameColumn(
                name: "id_trilha",
                table: "TRILHA",
                newName: "ID_TRILHA");

            migrationBuilder.RenameIndex(
                name: "IX_trilha_pet_id_pet",
                table: "TRILHA",
                newName: "IX_TRILHA_PET_ID_PET");

            migrationBuilder.RenameColumn(
                name: "num_tel",
                table: "TELEFONE",
                newName: "NUM_TEL");

            migrationBuilder.RenameColumn(
                name: "num_ddd",
                table: "TELEFONE",
                newName: "NUM_DDD");

            migrationBuilder.RenameColumn(
                name: "id_telefone",
                table: "TELEFONE",
                newName: "ID_TELEFONE");

            migrationBuilder.RenameColumn(
                name: "usuario_id_usuario",
                table: "TAREFA",
                newName: "USUARIO_ID_USUARIO");

            migrationBuilder.RenameColumn(
                name: "titulo",
                table: "TAREFA",
                newName: "TITULO");

            migrationBuilder.RenameColumn(
                name: "status_id_status",
                table: "TAREFA",
                newName: "STATUS_ID_STATUS");

            migrationBuilder.RenameColumn(
                name: "prazo",
                table: "TAREFA",
                newName: "PRAZO");

            migrationBuilder.RenameColumn(
                name: "pontos_tarefa",
                table: "TAREFA",
                newName: "PONTOS_TAREFA");

            migrationBuilder.RenameColumn(
                name: "pet_id_pet",
                table: "TAREFA",
                newName: "PET_ID_PET");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "TAREFA",
                newName: "DESCRICAO");

            migrationBuilder.RenameColumn(
                name: "criacao",
                table: "TAREFA",
                newName: "CRIACAO");

            migrationBuilder.RenameColumn(
                name: "conclusao",
                table: "TAREFA",
                newName: "CONCLUSAO");

            migrationBuilder.RenameColumn(
                name: "id_tarefa",
                table: "TAREFA",
                newName: "ID_TAREFA");

            migrationBuilder.RenameIndex(
                name: "IX_tarefa_usuario_id_usuario",
                table: "TAREFA",
                newName: "IX_TAREFA_USUARIO_ID_USUARIO");

            migrationBuilder.RenameIndex(
                name: "IX_tarefa_status_id_status",
                table: "TAREFA",
                newName: "IX_TAREFA_STATUS_ID_STATUS");

            migrationBuilder.RenameIndex(
                name: "IX_tarefa_pet_id_pet",
                table: "TAREFA",
                newName: "IX_TAREFA_PET_ID_PET");

            migrationBuilder.RenameColumn(
                name: "nome_status",
                table: "STATUS",
                newName: "NOME_STATUS");

            migrationBuilder.RenameColumn(
                name: "id_status",
                table: "STATUS",
                newName: "ID_STATUS");

            migrationBuilder.RenameColumn(
                name: "nome_raca",
                table: "RACA",
                newName: "NOME_RACA");

            migrationBuilder.RenameColumn(
                name: "id_raca",
                table: "RACA",
                newName: "ID_RACA");

            migrationBuilder.RenameColumn(
                name: "sexo",
                table: "PET",
                newName: "SEXO");

            migrationBuilder.RenameColumn(
                name: "raca_id_raca",
                table: "PET",
                newName: "RACA_ID_RACA");

            migrationBuilder.RenameColumn(
                name: "porte",
                table: "PET",
                newName: "PORTE");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "PET",
                newName: "NOME");

            migrationBuilder.RenameColumn(
                name: "data_nasc",
                table: "PET",
                newName: "DATA_NASC");

            migrationBuilder.RenameColumn(
                name: "castrado",
                table: "PET",
                newName: "CASTRADO");

            migrationBuilder.RenameColumn(
                name: "id_pet",
                table: "PET",
                newName: "ID_PET");

            migrationBuilder.RenameIndex(
                name: "IX_pet_raca_id_raca",
                table: "PET",
                newName: "IX_PET_RACA_ID_RACA");

            migrationBuilder.RenameColumn(
                name: "trilha_id_trilha",
                table: "MODULO",
                newName: "TRILHA_ID_TRILHA");

            migrationBuilder.RenameColumn(
                name: "tempo_conclusao",
                table: "MODULO",
                newName: "TEMPO_CONCLUSAO");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "MODULO",
                newName: "NOME");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "MODULO",
                newName: "DESCRICAO");

            migrationBuilder.RenameColumn(
                name: "id_modulo",
                table: "MODULO",
                newName: "ID_MODULO");

            migrationBuilder.RenameIndex(
                name: "IX_modulo_trilha_id_trilha",
                table: "MODULO",
                newName: "IX_MODULO_TRILHA_ID_TRILHA");

            migrationBuilder.RenameColumn(
                name: "tipo_hist",
                table: "HISTORICO",
                newName: "TIPO_HIST");

            migrationBuilder.RenameColumn(
                name: "pet_id_pet",
                table: "HISTORICO",
                newName: "PET_ID_PET");

            migrationBuilder.RenameColumn(
                name: "data_hist",
                table: "HISTORICO",
                newName: "DATA_HIST");

            migrationBuilder.RenameColumn(
                name: "id_hist",
                table: "HISTORICO",
                newName: "ID_HIST");

            migrationBuilder.RenameIndex(
                name: "IX_historico_pet_id_pet",
                table: "HISTORICO",
                newName: "IX_HISTORICO_PET_ID_PET");

            migrationBuilder.RenameColumn(
                name: "nome_estado",
                table: "ESTADO",
                newName: "NOME_ESTADO");

            migrationBuilder.RenameColumn(
                name: "id_estado",
                table: "ESTADO",
                newName: "ID_ESTADO");

            migrationBuilder.RenameColumn(
                name: "rua",
                table: "ENDERECO",
                newName: "RUA");

            migrationBuilder.RenameColumn(
                name: "numero",
                table: "ENDERECO",
                newName: "NUMERO");

            migrationBuilder.RenameColumn(
                name: "cep",
                table: "ENDERECO",
                newName: "CEP");

            migrationBuilder.RenameColumn(
                name: "bairro_id_bairro",
                table: "ENDERECO",
                newName: "BAIRRO_ID_BAIRRO");

            migrationBuilder.RenameColumn(
                name: "id_endereco",
                table: "ENDERECO",
                newName: "ID_ENDERECO");

            migrationBuilder.RenameIndex(
                name: "IX_endereco_bairro_id_bairro",
                table: "ENDERECO",
                newName: "IX_ENDERECO_BAIRRO_ID_BAIRRO");

            migrationBuilder.RenameColumn(
                name: "nome_cidade",
                table: "CIDADE",
                newName: "NOME_CIDADE");

            migrationBuilder.RenameColumn(
                name: "estado_id_estado",
                table: "CIDADE",
                newName: "ESTADO_ID_ESTADO");

            migrationBuilder.RenameColumn(
                name: "id_cidade",
                table: "CIDADE",
                newName: "ID_CIDADE");

            migrationBuilder.RenameIndex(
                name: "IX_cidade_estado_id_estado",
                table: "CIDADE",
                newName: "IX_CIDADE_ESTADO_ID_ESTADO");

            migrationBuilder.RenameColumn(
                name: "nome_bairro",
                table: "BAIRRO",
                newName: "NOME_BAIRRO");

            migrationBuilder.RenameColumn(
                name: "cidade_id_cidade",
                table: "BAIRRO",
                newName: "CIDADE_ID_CIDADE");

            migrationBuilder.RenameColumn(
                name: "id_bairro",
                table: "BAIRRO",
                newName: "ID_BAIRRO");

            migrationBuilder.RenameIndex(
                name: "IX_bairro_cidade_id_cidade",
                table: "BAIRRO",
                newName: "IX_BAIRRO_CIDADE_ID_CIDADE");

            migrationBuilder.RenameColumn(
                name: "pontos_aula",
                table: "AULA",
                newName: "PONTOS_AULA");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "AULA",
                newName: "NOME");

            migrationBuilder.RenameColumn(
                name: "modulo_id_modulo",
                table: "AULA",
                newName: "MODULO_ID_MODULO");

            migrationBuilder.RenameColumn(
                name: "dificuldade",
                table: "AULA",
                newName: "DIFICULDADE");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "AULA",
                newName: "DESCRICAO");

            migrationBuilder.RenameColumn(
                name: "conteudo",
                table: "AULA",
                newName: "CONTEUDO");

            migrationBuilder.RenameColumn(
                name: "concluida",
                table: "AULA",
                newName: "CONCLUIDA");

            migrationBuilder.RenameColumn(
                name: "id_aula",
                table: "AULA",
                newName: "ID_AULA");

            migrationBuilder.RenameIndex(
                name: "IX_aula_modulo_id_modulo",
                table: "AULA",
                newName: "IX_AULA_MODULO_ID_MODULO");

            migrationBuilder.AlterColumn<string>(
                name: "SENHA",
                table: "USUARIO",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(60)",
                oldMaxLength: 60);

            migrationBuilder.AddColumn<string>(
                name: "SALT",
                table: "USUARIO",
                type: "NVARCHAR2(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_USUARIO_PET",
                table: "USUARIO_PET",
                columns: new[] { "USUARIO_ID_USUARIO", "PET_ID_PET" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_USUARIO_ENDERECO",
                table: "USUARIO_ENDERECO",
                columns: new[] { "USUARIO_ID_USUARIO", "ENDERECO_ID_ENDERECO" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_USUARIO",
                table: "USUARIO",
                column: "ID_USUARIO");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TRILHA",
                table: "TRILHA",
                column: "ID_TRILHA");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TELEFONE",
                table: "TELEFONE",
                column: "ID_TELEFONE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TAREFA",
                table: "TAREFA",
                column: "ID_TAREFA");

            migrationBuilder.AddPrimaryKey(
                name: "PK_STATUS",
                table: "STATUS",
                column: "ID_STATUS");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RACA",
                table: "RACA",
                column: "ID_RACA");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PET",
                table: "PET",
                column: "ID_PET");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MODULO",
                table: "MODULO",
                column: "ID_MODULO");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HISTORICO",
                table: "HISTORICO",
                column: "ID_HIST");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ESTADO",
                table: "ESTADO",
                column: "ID_ESTADO");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ENDERECO",
                table: "ENDERECO",
                column: "ID_ENDERECO");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CIDADE",
                table: "CIDADE",
                column: "ID_CIDADE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BAIRRO",
                table: "BAIRRO",
                column: "ID_BAIRRO");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AULA",
                table: "AULA",
                column: "ID_AULA");

            migrationBuilder.AddForeignKey(
                name: "FK_AULA_MODULO_MODULO_ID_MODULO",
                table: "AULA",
                column: "MODULO_ID_MODULO",
                principalTable: "MODULO",
                principalColumn: "ID_MODULO",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BAIRRO_CIDADE_CIDADE_ID_CIDADE",
                table: "BAIRRO",
                column: "CIDADE_ID_CIDADE",
                principalTable: "CIDADE",
                principalColumn: "ID_CIDADE",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CIDADE_ESTADO_ESTADO_ID_ESTADO",
                table: "CIDADE",
                column: "ESTADO_ID_ESTADO",
                principalTable: "ESTADO",
                principalColumn: "ID_ESTADO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ENDERECO_BAIRRO_BAIRRO_ID_BAIRRO",
                table: "ENDERECO",
                column: "BAIRRO_ID_BAIRRO",
                principalTable: "BAIRRO",
                principalColumn: "ID_BAIRRO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HISTORICO_PET_PET_ID_PET",
                table: "HISTORICO",
                column: "PET_ID_PET",
                principalTable: "PET",
                principalColumn: "ID_PET",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MODULO_TRILHA_TRILHA_ID_TRILHA",
                table: "MODULO",
                column: "TRILHA_ID_TRILHA",
                principalTable: "TRILHA",
                principalColumn: "ID_TRILHA",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PET_RACA_RACA_ID_RACA",
                table: "PET",
                column: "RACA_ID_RACA",
                principalTable: "RACA",
                principalColumn: "ID_RACA",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TAREFA_PET_PET_ID_PET",
                table: "TAREFA",
                column: "PET_ID_PET",
                principalTable: "PET",
                principalColumn: "ID_PET",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TAREFA_STATUS_STATUS_ID_STATUS",
                table: "TAREFA",
                column: "STATUS_ID_STATUS",
                principalTable: "STATUS",
                principalColumn: "ID_STATUS",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TAREFA_USUARIO_USUARIO_ID_USUARIO",
                table: "TAREFA",
                column: "USUARIO_ID_USUARIO",
                principalTable: "USUARIO",
                principalColumn: "ID_USUARIO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TRILHA_PET_PET_ID_PET",
                table: "TRILHA",
                column: "PET_ID_PET",
                principalTable: "PET",
                principalColumn: "ID_PET",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_TELEFONE_TELEFONE_ID_TELEFONE",
                table: "USUARIO",
                column: "TELEFONE_ID_TELEFONE",
                principalTable: "TELEFONE",
                principalColumn: "ID_TELEFONE",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_ENDERECO_ENDERECO_ENDERECO_ID_ENDERECO",
                table: "USUARIO_ENDERECO",
                column: "ENDERECO_ID_ENDERECO",
                principalTable: "ENDERECO",
                principalColumn: "ID_ENDERECO",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_ENDERECO_USUARIO_USUARIO_ID_USUARIO",
                table: "USUARIO_ENDERECO",
                column: "USUARIO_ID_USUARIO",
                principalTable: "USUARIO",
                principalColumn: "ID_USUARIO",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_PET_PET_PET_ID_PET",
                table: "USUARIO_PET",
                column: "PET_ID_PET",
                principalTable: "PET",
                principalColumn: "ID_PET",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_PET_USUARIO_USUARIO_ID_USUARIO",
                table: "USUARIO_PET",
                column: "USUARIO_ID_USUARIO",
                principalTable: "USUARIO",
                principalColumn: "ID_USUARIO",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AULA_MODULO_MODULO_ID_MODULO",
                table: "AULA");

            migrationBuilder.DropForeignKey(
                name: "FK_BAIRRO_CIDADE_CIDADE_ID_CIDADE",
                table: "BAIRRO");

            migrationBuilder.DropForeignKey(
                name: "FK_CIDADE_ESTADO_ESTADO_ID_ESTADO",
                table: "CIDADE");

            migrationBuilder.DropForeignKey(
                name: "FK_ENDERECO_BAIRRO_BAIRRO_ID_BAIRRO",
                table: "ENDERECO");

            migrationBuilder.DropForeignKey(
                name: "FK_HISTORICO_PET_PET_ID_PET",
                table: "HISTORICO");

            migrationBuilder.DropForeignKey(
                name: "FK_MODULO_TRILHA_TRILHA_ID_TRILHA",
                table: "MODULO");

            migrationBuilder.DropForeignKey(
                name: "FK_PET_RACA_RACA_ID_RACA",
                table: "PET");

            migrationBuilder.DropForeignKey(
                name: "FK_TAREFA_PET_PET_ID_PET",
                table: "TAREFA");

            migrationBuilder.DropForeignKey(
                name: "FK_TAREFA_STATUS_STATUS_ID_STATUS",
                table: "TAREFA");

            migrationBuilder.DropForeignKey(
                name: "FK_TAREFA_USUARIO_USUARIO_ID_USUARIO",
                table: "TAREFA");

            migrationBuilder.DropForeignKey(
                name: "FK_TRILHA_PET_PET_ID_PET",
                table: "TRILHA");

            migrationBuilder.DropForeignKey(
                name: "FK_USUARIO_TELEFONE_TELEFONE_ID_TELEFONE",
                table: "USUARIO");

            migrationBuilder.DropForeignKey(
                name: "FK_USUARIO_ENDERECO_ENDERECO_ENDERECO_ID_ENDERECO",
                table: "USUARIO_ENDERECO");

            migrationBuilder.DropForeignKey(
                name: "FK_USUARIO_ENDERECO_USUARIO_USUARIO_ID_USUARIO",
                table: "USUARIO_ENDERECO");

            migrationBuilder.DropForeignKey(
                name: "FK_USUARIO_PET_PET_PET_ID_PET",
                table: "USUARIO_PET");

            migrationBuilder.DropForeignKey(
                name: "FK_USUARIO_PET_USUARIO_USUARIO_ID_USUARIO",
                table: "USUARIO_PET");

            migrationBuilder.DropPrimaryKey(
                name: "PK_USUARIO_PET",
                table: "USUARIO_PET");

            migrationBuilder.DropPrimaryKey(
                name: "PK_USUARIO_ENDERECO",
                table: "USUARIO_ENDERECO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_USUARIO",
                table: "USUARIO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TRILHA",
                table: "TRILHA");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TELEFONE",
                table: "TELEFONE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TAREFA",
                table: "TAREFA");

            migrationBuilder.DropPrimaryKey(
                name: "PK_STATUS",
                table: "STATUS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RACA",
                table: "RACA");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PET",
                table: "PET");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MODULO",
                table: "MODULO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HISTORICO",
                table: "HISTORICO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ESTADO",
                table: "ESTADO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ENDERECO",
                table: "ENDERECO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CIDADE",
                table: "CIDADE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BAIRRO",
                table: "BAIRRO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AULA",
                table: "AULA");

            migrationBuilder.DropColumn(
                name: "SALT",
                table: "USUARIO");

            migrationBuilder.RenameTable(
                name: "USUARIO_PET",
                newName: "usuario_pet");

            migrationBuilder.RenameTable(
                name: "USUARIO_ENDERECO",
                newName: "usuario_endereco");

            migrationBuilder.RenameTable(
                name: "USUARIO",
                newName: "usuario");

            migrationBuilder.RenameTable(
                name: "TRILHA",
                newName: "trilha");

            migrationBuilder.RenameTable(
                name: "TELEFONE",
                newName: "telefone");

            migrationBuilder.RenameTable(
                name: "TAREFA",
                newName: "tarefa");

            migrationBuilder.RenameTable(
                name: "STATUS",
                newName: "status");

            migrationBuilder.RenameTable(
                name: "RACA",
                newName: "raca");

            migrationBuilder.RenameTable(
                name: "PET",
                newName: "pet");

            migrationBuilder.RenameTable(
                name: "MODULO",
                newName: "modulo");

            migrationBuilder.RenameTable(
                name: "HISTORICO",
                newName: "historico");

            migrationBuilder.RenameTable(
                name: "ESTADO",
                newName: "estado");

            migrationBuilder.RenameTable(
                name: "ENDERECO",
                newName: "endereco");

            migrationBuilder.RenameTable(
                name: "CIDADE",
                newName: "cidade");

            migrationBuilder.RenameTable(
                name: "BAIRRO",
                newName: "bairro");

            migrationBuilder.RenameTable(
                name: "AULA",
                newName: "aula");

            migrationBuilder.RenameColumn(
                name: "RESPON_PRINC",
                table: "usuario_pet",
                newName: "respon_princ");

            migrationBuilder.RenameColumn(
                name: "PET_ID_PET",
                table: "usuario_pet",
                newName: "pet_id_pet");

            migrationBuilder.RenameColumn(
                name: "USUARIO_ID_USUARIO",
                table: "usuario_pet",
                newName: "usuario_id_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_USUARIO_PET_PET_ID_PET",
                table: "usuario_pet",
                newName: "IX_usuario_pet_pet_id_pet");

            migrationBuilder.RenameColumn(
                name: "ENDERECO_ID_ENDERECO",
                table: "usuario_endereco",
                newName: "endereco_id_endereco");

            migrationBuilder.RenameColumn(
                name: "USUARIO_ID_USUARIO",
                table: "usuario_endereco",
                newName: "usuario_id_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_USUARIO_ENDERECO_ENDERECO_ID_ENDERECO",
                table: "usuario_endereco",
                newName: "IX_usuario_endereco_endereco_id_endereco");

            migrationBuilder.RenameColumn(
                name: "TELEFONE_ID_TELEFONE",
                table: "usuario",
                newName: "telefone_id_telefone");

            migrationBuilder.RenameColumn(
                name: "SENHA",
                table: "usuario",
                newName: "senha");

            migrationBuilder.RenameColumn(
                name: "ROLE",
                table: "usuario",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "NOME",
                table: "usuario",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "EMAIL",
                table: "usuario",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "ID_USUARIO",
                table: "usuario",
                newName: "id_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_USUARIO_TELEFONE_ID_TELEFONE",
                table: "usuario",
                newName: "IX_usuario_telefone_id_telefone");

            migrationBuilder.RenameIndex(
                name: "IX_USUARIO_EMAIL",
                table: "usuario",
                newName: "IX_usuario_email");

            migrationBuilder.RenameColumn(
                name: "PET_ID_PET",
                table: "trilha",
                newName: "pet_id_pet");

            migrationBuilder.RenameColumn(
                name: "NOME",
                table: "trilha",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "DESCRICAO",
                table: "trilha",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "ID_TRILHA",
                table: "trilha",
                newName: "id_trilha");

            migrationBuilder.RenameIndex(
                name: "IX_TRILHA_PET_ID_PET",
                table: "trilha",
                newName: "IX_trilha_pet_id_pet");

            migrationBuilder.RenameColumn(
                name: "NUM_TEL",
                table: "telefone",
                newName: "num_tel");

            migrationBuilder.RenameColumn(
                name: "NUM_DDD",
                table: "telefone",
                newName: "num_ddd");

            migrationBuilder.RenameColumn(
                name: "ID_TELEFONE",
                table: "telefone",
                newName: "id_telefone");

            migrationBuilder.RenameColumn(
                name: "USUARIO_ID_USUARIO",
                table: "tarefa",
                newName: "usuario_id_usuario");

            migrationBuilder.RenameColumn(
                name: "TITULO",
                table: "tarefa",
                newName: "titulo");

            migrationBuilder.RenameColumn(
                name: "STATUS_ID_STATUS",
                table: "tarefa",
                newName: "status_id_status");

            migrationBuilder.RenameColumn(
                name: "PRAZO",
                table: "tarefa",
                newName: "prazo");

            migrationBuilder.RenameColumn(
                name: "PONTOS_TAREFA",
                table: "tarefa",
                newName: "pontos_tarefa");

            migrationBuilder.RenameColumn(
                name: "PET_ID_PET",
                table: "tarefa",
                newName: "pet_id_pet");

            migrationBuilder.RenameColumn(
                name: "DESCRICAO",
                table: "tarefa",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "CRIACAO",
                table: "tarefa",
                newName: "criacao");

            migrationBuilder.RenameColumn(
                name: "CONCLUSAO",
                table: "tarefa",
                newName: "conclusao");

            migrationBuilder.RenameColumn(
                name: "ID_TAREFA",
                table: "tarefa",
                newName: "id_tarefa");

            migrationBuilder.RenameIndex(
                name: "IX_TAREFA_USUARIO_ID_USUARIO",
                table: "tarefa",
                newName: "IX_tarefa_usuario_id_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_TAREFA_STATUS_ID_STATUS",
                table: "tarefa",
                newName: "IX_tarefa_status_id_status");

            migrationBuilder.RenameIndex(
                name: "IX_TAREFA_PET_ID_PET",
                table: "tarefa",
                newName: "IX_tarefa_pet_id_pet");

            migrationBuilder.RenameColumn(
                name: "NOME_STATUS",
                table: "status",
                newName: "nome_status");

            migrationBuilder.RenameColumn(
                name: "ID_STATUS",
                table: "status",
                newName: "id_status");

            migrationBuilder.RenameColumn(
                name: "NOME_RACA",
                table: "raca",
                newName: "nome_raca");

            migrationBuilder.RenameColumn(
                name: "ID_RACA",
                table: "raca",
                newName: "id_raca");

            migrationBuilder.RenameColumn(
                name: "SEXO",
                table: "pet",
                newName: "sexo");

            migrationBuilder.RenameColumn(
                name: "RACA_ID_RACA",
                table: "pet",
                newName: "raca_id_raca");

            migrationBuilder.RenameColumn(
                name: "PORTE",
                table: "pet",
                newName: "porte");

            migrationBuilder.RenameColumn(
                name: "NOME",
                table: "pet",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "DATA_NASC",
                table: "pet",
                newName: "data_nasc");

            migrationBuilder.RenameColumn(
                name: "CASTRADO",
                table: "pet",
                newName: "castrado");

            migrationBuilder.RenameColumn(
                name: "ID_PET",
                table: "pet",
                newName: "id_pet");

            migrationBuilder.RenameIndex(
                name: "IX_PET_RACA_ID_RACA",
                table: "pet",
                newName: "IX_pet_raca_id_raca");

            migrationBuilder.RenameColumn(
                name: "TRILHA_ID_TRILHA",
                table: "modulo",
                newName: "trilha_id_trilha");

            migrationBuilder.RenameColumn(
                name: "TEMPO_CONCLUSAO",
                table: "modulo",
                newName: "tempo_conclusao");

            migrationBuilder.RenameColumn(
                name: "NOME",
                table: "modulo",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "DESCRICAO",
                table: "modulo",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "ID_MODULO",
                table: "modulo",
                newName: "id_modulo");

            migrationBuilder.RenameIndex(
                name: "IX_MODULO_TRILHA_ID_TRILHA",
                table: "modulo",
                newName: "IX_modulo_trilha_id_trilha");

            migrationBuilder.RenameColumn(
                name: "TIPO_HIST",
                table: "historico",
                newName: "tipo_hist");

            migrationBuilder.RenameColumn(
                name: "PET_ID_PET",
                table: "historico",
                newName: "pet_id_pet");

            migrationBuilder.RenameColumn(
                name: "DATA_HIST",
                table: "historico",
                newName: "data_hist");

            migrationBuilder.RenameColumn(
                name: "ID_HIST",
                table: "historico",
                newName: "id_hist");

            migrationBuilder.RenameIndex(
                name: "IX_HISTORICO_PET_ID_PET",
                table: "historico",
                newName: "IX_historico_pet_id_pet");

            migrationBuilder.RenameColumn(
                name: "NOME_ESTADO",
                table: "estado",
                newName: "nome_estado");

            migrationBuilder.RenameColumn(
                name: "ID_ESTADO",
                table: "estado",
                newName: "id_estado");

            migrationBuilder.RenameColumn(
                name: "RUA",
                table: "endereco",
                newName: "rua");

            migrationBuilder.RenameColumn(
                name: "NUMERO",
                table: "endereco",
                newName: "numero");

            migrationBuilder.RenameColumn(
                name: "CEP",
                table: "endereco",
                newName: "cep");

            migrationBuilder.RenameColumn(
                name: "BAIRRO_ID_BAIRRO",
                table: "endereco",
                newName: "bairro_id_bairro");

            migrationBuilder.RenameColumn(
                name: "ID_ENDERECO",
                table: "endereco",
                newName: "id_endereco");

            migrationBuilder.RenameIndex(
                name: "IX_ENDERECO_BAIRRO_ID_BAIRRO",
                table: "endereco",
                newName: "IX_endereco_bairro_id_bairro");

            migrationBuilder.RenameColumn(
                name: "NOME_CIDADE",
                table: "cidade",
                newName: "nome_cidade");

            migrationBuilder.RenameColumn(
                name: "ESTADO_ID_ESTADO",
                table: "cidade",
                newName: "estado_id_estado");

            migrationBuilder.RenameColumn(
                name: "ID_CIDADE",
                table: "cidade",
                newName: "id_cidade");

            migrationBuilder.RenameIndex(
                name: "IX_CIDADE_ESTADO_ID_ESTADO",
                table: "cidade",
                newName: "IX_cidade_estado_id_estado");

            migrationBuilder.RenameColumn(
                name: "NOME_BAIRRO",
                table: "bairro",
                newName: "nome_bairro");

            migrationBuilder.RenameColumn(
                name: "CIDADE_ID_CIDADE",
                table: "bairro",
                newName: "cidade_id_cidade");

            migrationBuilder.RenameColumn(
                name: "ID_BAIRRO",
                table: "bairro",
                newName: "id_bairro");

            migrationBuilder.RenameIndex(
                name: "IX_BAIRRO_CIDADE_ID_CIDADE",
                table: "bairro",
                newName: "IX_bairro_cidade_id_cidade");

            migrationBuilder.RenameColumn(
                name: "PONTOS_AULA",
                table: "aula",
                newName: "pontos_aula");

            migrationBuilder.RenameColumn(
                name: "NOME",
                table: "aula",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "MODULO_ID_MODULO",
                table: "aula",
                newName: "modulo_id_modulo");

            migrationBuilder.RenameColumn(
                name: "DIFICULDADE",
                table: "aula",
                newName: "dificuldade");

            migrationBuilder.RenameColumn(
                name: "DESCRICAO",
                table: "aula",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "CONTEUDO",
                table: "aula",
                newName: "conteudo");

            migrationBuilder.RenameColumn(
                name: "CONCLUIDA",
                table: "aula",
                newName: "concluida");

            migrationBuilder.RenameColumn(
                name: "ID_AULA",
                table: "aula",
                newName: "id_aula");

            migrationBuilder.RenameIndex(
                name: "IX_AULA_MODULO_ID_MODULO",
                table: "aula",
                newName: "IX_aula_modulo_id_modulo");

            migrationBuilder.AlterColumn<string>(
                name: "senha",
                table: "usuario",
                type: "NVARCHAR2(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuario_pet",
                table: "usuario_pet",
                columns: new[] { "usuario_id_usuario", "pet_id_pet" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuario_endereco",
                table: "usuario_endereco",
                columns: new[] { "usuario_id_usuario", "endereco_id_endereco" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuario",
                table: "usuario",
                column: "id_usuario");

            migrationBuilder.AddPrimaryKey(
                name: "PK_trilha",
                table: "trilha",
                column: "id_trilha");

            migrationBuilder.AddPrimaryKey(
                name: "PK_telefone",
                table: "telefone",
                column: "id_telefone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tarefa",
                table: "tarefa",
                column: "id_tarefa");

            migrationBuilder.AddPrimaryKey(
                name: "PK_status",
                table: "status",
                column: "id_status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_raca",
                table: "raca",
                column: "id_raca");

            migrationBuilder.AddPrimaryKey(
                name: "PK_pet",
                table: "pet",
                column: "id_pet");

            migrationBuilder.AddPrimaryKey(
                name: "PK_modulo",
                table: "modulo",
                column: "id_modulo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_historico",
                table: "historico",
                column: "id_hist");

            migrationBuilder.AddPrimaryKey(
                name: "PK_estado",
                table: "estado",
                column: "id_estado");

            migrationBuilder.AddPrimaryKey(
                name: "PK_endereco",
                table: "endereco",
                column: "id_endereco");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cidade",
                table: "cidade",
                column: "id_cidade");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bairro",
                table: "bairro",
                column: "id_bairro");

            migrationBuilder.AddPrimaryKey(
                name: "PK_aula",
                table: "aula",
                column: "id_aula");

            migrationBuilder.AddForeignKey(
                name: "FK_aula_modulo_modulo_id_modulo",
                table: "aula",
                column: "modulo_id_modulo",
                principalTable: "modulo",
                principalColumn: "id_modulo",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_bairro_cidade_cidade_id_cidade",
                table: "bairro",
                column: "cidade_id_cidade",
                principalTable: "cidade",
                principalColumn: "id_cidade",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cidade_estado_estado_id_estado",
                table: "cidade",
                column: "estado_id_estado",
                principalTable: "estado",
                principalColumn: "id_estado",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_endereco_bairro_bairro_id_bairro",
                table: "endereco",
                column: "bairro_id_bairro",
                principalTable: "bairro",
                principalColumn: "id_bairro",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_historico_pet_pet_id_pet",
                table: "historico",
                column: "pet_id_pet",
                principalTable: "pet",
                principalColumn: "id_pet",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_modulo_trilha_trilha_id_trilha",
                table: "modulo",
                column: "trilha_id_trilha",
                principalTable: "trilha",
                principalColumn: "id_trilha",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_pet_raca_raca_id_raca",
                table: "pet",
                column: "raca_id_raca",
                principalTable: "raca",
                principalColumn: "id_raca",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tarefa_pet_pet_id_pet",
                table: "tarefa",
                column: "pet_id_pet",
                principalTable: "pet",
                principalColumn: "id_pet",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tarefa_status_status_id_status",
                table: "tarefa",
                column: "status_id_status",
                principalTable: "status",
                principalColumn: "id_status",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tarefa_usuario_usuario_id_usuario",
                table: "tarefa",
                column: "usuario_id_usuario",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_trilha_pet_pet_id_pet",
                table: "trilha",
                column: "pet_id_pet",
                principalTable: "pet",
                principalColumn: "id_pet",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_telefone_telefone_id_telefone",
                table: "usuario",
                column: "telefone_id_telefone",
                principalTable: "telefone",
                principalColumn: "id_telefone",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_endereco_endereco_endereco_id_endereco",
                table: "usuario_endereco",
                column: "endereco_id_endereco",
                principalTable: "endereco",
                principalColumn: "id_endereco",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_endereco_usuario_usuario_id_usuario",
                table: "usuario_endereco",
                column: "usuario_id_usuario",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_pet_pet_pet_id_pet",
                table: "usuario_pet",
                column: "pet_id_pet",
                principalTable: "pet",
                principalColumn: "id_pet",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usuario_pet_usuario_usuario_id_usuario",
                table: "usuario_pet",
                column: "usuario_id_usuario",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
