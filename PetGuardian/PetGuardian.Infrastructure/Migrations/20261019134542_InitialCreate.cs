using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetGuardian.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
BEGIN
  FOR t IN (SELECT table_name FROM user_tables WHERE table_name <> '__EFMigrationsHistory') LOOP
    EXECUTE IMMEDIATE 'DROP TABLE ""' || t.table_name || '"" CASCADE CONSTRAINTS PURGE';
  END LOOP;
END;");

            migrationBuilder.CreateTable(
                name: "ESTADO",
                columns: table => new
                {
                    ID_ESTADO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME_ESTADO = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESTADO", x => x.ID_ESTADO);
                });

            migrationBuilder.CreateTable(
                name: "RACA",
                columns: table => new
                {
                    ID_RACA = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME_RACA = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RACA", x => x.ID_RACA);
                });

            migrationBuilder.CreateTable(
                name: "STATUS",
                columns: table => new
                {
                    ID_STATUS = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME_STATUS = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STATUS", x => x.ID_STATUS);
                });

            migrationBuilder.CreateTable(
                name: "TELEFONE",
                columns: table => new
                {
                    ID_TELEFONE = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NUM_DDD = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                    NUM_TEL = table.Column<string>(type: "NVARCHAR2(9)", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TELEFONE", x => x.ID_TELEFONE);
                });

            migrationBuilder.CreateTable(
                name: "CIDADE",
                columns: table => new
                {
                    ID_CIDADE = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME_CIDADE = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    ESTADO_ID_ESTADO = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIDADE", x => x.ID_CIDADE);
                    table.ForeignKey(
                        name: "FK_CIDADE_ESTADO_ESTADO_ID_ESTADO",
                        column: x => x.ESTADO_ID_ESTADO,
                        principalTable: "ESTADO",
                        principalColumn: "ID_ESTADO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PET",
                columns: table => new
                {
                    ID_PET = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    DATA_NASC = table.Column<DateTime>(type: "DATE", nullable: false),
                    SEXO = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                    PORTE = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    CASTRADO = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    RACA_ID_RACA = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PET", x => x.ID_PET);
                    table.ForeignKey(
                        name: "FK_PET_RACA_RACA_ID_RACA",
                        column: x => x.RACA_ID_RACA,
                        principalTable: "RACA",
                        principalColumn: "ID_RACA",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BAIRRO",
                columns: table => new
                {
                    ID_BAIRRO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME_BAIRRO = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    CIDADE_ID_CIDADE = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAIRRO", x => x.ID_BAIRRO);
                    table.ForeignKey(
                        name: "FK_BAIRRO_CIDADE_CIDADE_ID_CIDADE",
                        column: x => x.CIDADE_ID_CIDADE,
                        principalTable: "CIDADE",
                        principalColumn: "ID_CIDADE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HISTORICO",
                columns: table => new
                {
                    ID_HIST = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TIPO_HIST = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    DATA_HIST = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PET_ID_PET = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HISTORICO", x => x.ID_HIST);
                    table.ForeignKey(
                        name: "FK_HISTORICO_PET_PET_ID_PET",
                        column: x => x.PET_ID_PET,
                        principalTable: "PET",
                        principalColumn: "ID_PET",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRILHA",
                columns: table => new
                {
                    ID_TRILHA = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    PET_ID_PET = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRILHA", x => x.ID_TRILHA);
                    table.ForeignKey(
                        name: "FK_TRILHA_PET_PET_ID_PET",
                        column: x => x.PET_ID_PET,
                        principalTable: "PET",
                        principalColumn: "ID_PET",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ENDERECO",
                columns: table => new
                {
                    ID_ENDERECO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CEP = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    RUA = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    NUMERO = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: false),
                    BAIRRO_ID_BAIRRO = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENDERECO", x => x.ID_ENDERECO);
                    table.ForeignKey(
                        name: "FK_ENDERECO_BAIRRO_BAIRRO_ID_BAIRRO",
                        column: x => x.BAIRRO_ID_BAIRRO,
                        principalTable: "BAIRRO",
                        principalColumn: "ID_BAIRRO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MODULO",
                columns: table => new
                {
                    ID_MODULO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    TEMPO_CONCLUSAO = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    TRILHA_ID_TRILHA = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MODULO", x => x.ID_MODULO);
                    table.ForeignKey(
                        name: "FK_MODULO_TRILHA_TRILHA_ID_TRILHA",
                        column: x => x.TRILHA_ID_TRILHA,
                        principalTable: "TRILHA",
                        principalColumn: "ID_TRILHA",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    ID_USUARIO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    SENHA = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SALT = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    ROLE = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    TELEFONE_ID_TELEFONE = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ENDERECO_ID_ENDERECO = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.ID_USUARIO);
                    table.ForeignKey(
                        name: "FK_USUARIO_ENDERECO_ENDERECO_ID_ENDERECO",
                        column: x => x.ENDERECO_ID_ENDERECO,
                        principalTable: "ENDERECO",
                        principalColumn: "ID_ENDERECO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_USUARIO_TELEFONE_TELEFONE_ID_TELEFONE",
                        column: x => x.TELEFONE_ID_TELEFONE,
                        principalTable: "TELEFONE",
                        principalColumn: "ID_TELEFONE",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AULA",
                columns: table => new
                {
                    ID_AULA = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    PONTOS_AULA = table.Column<short>(type: "NUMBER(5)", nullable: false),
                    DIFICULDADE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    CONCLUIDA = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    MODULO_ID_MODULO = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AULA", x => x.ID_AULA);
                    table.ForeignKey(
                        name: "FK_AULA_MODULO_MODULO_ID_MODULO",
                        column: x => x.MODULO_ID_MODULO,
                        principalTable: "MODULO",
                        principalColumn: "ID_MODULO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TAREFA",
                columns: table => new
                {
                    ID_TAREFA = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TITULO = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    PONTOS_TAREFA = table.Column<byte>(type: "NUMBER(3)", nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    CRIACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PRAZO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CONCLUSAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    USUARIO_ID_USUARIO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PET_ID_PET = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    STATUS_ID_STATUS = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAREFA", x => x.ID_TAREFA);
                    table.ForeignKey(
                        name: "FK_TAREFA_PET_PET_ID_PET",
                        column: x => x.PET_ID_PET,
                        principalTable: "PET",
                        principalColumn: "ID_PET",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TAREFA_STATUS_STATUS_ID_STATUS",
                        column: x => x.STATUS_ID_STATUS,
                        principalTable: "STATUS",
                        principalColumn: "ID_STATUS",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TAREFA_USUARIO_USUARIO_ID_USUARIO",
                        column: x => x.USUARIO_ID_USUARIO,
                        principalTable: "USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO_PET",
                columns: table => new
                {
                    USUARIO_ID_USUARIO = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PET_ID_PET = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RESPON_PRINC = table.Column<int>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO_PET", x => new { x.USUARIO_ID_USUARIO, x.PET_ID_PET });
                    table.ForeignKey(
                        name: "FK_USUARIO_PET_PET_PET_ID_PET",
                        column: x => x.PET_ID_PET,
                        principalTable: "PET",
                        principalColumn: "ID_PET",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USUARIO_PET_USUARIO_USUARIO_ID_USUARIO",
                        column: x => x.USUARIO_ID_USUARIO,
                        principalTable: "USUARIO",
                        principalColumn: "ID_USUARIO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AULA_MODULO_ID_MODULO",
                table: "AULA",
                column: "MODULO_ID_MODULO");

            migrationBuilder.CreateIndex(
                name: "IX_BAIRRO_CIDADE_ID_CIDADE",
                table: "BAIRRO",
                column: "CIDADE_ID_CIDADE");

            migrationBuilder.CreateIndex(
                name: "IX_CIDADE_ESTADO_ID_ESTADO",
                table: "CIDADE",
                column: "ESTADO_ID_ESTADO");

            migrationBuilder.CreateIndex(
                name: "IX_ENDERECO_BAIRRO_ID_BAIRRO",
                table: "ENDERECO",
                column: "BAIRRO_ID_BAIRRO");

            migrationBuilder.CreateIndex(
                name: "IX_HISTORICO_PET_ID_PET",
                table: "HISTORICO",
                column: "PET_ID_PET");

            migrationBuilder.CreateIndex(
                name: "IX_MODULO_TRILHA_ID_TRILHA",
                table: "MODULO",
                column: "TRILHA_ID_TRILHA");

            migrationBuilder.CreateIndex(
                name: "IX_PET_RACA_ID_RACA",
                table: "PET",
                column: "RACA_ID_RACA");

            migrationBuilder.CreateIndex(
                name: "IX_TAREFA_PET_ID_PET",
                table: "TAREFA",
                column: "PET_ID_PET");

            migrationBuilder.CreateIndex(
                name: "IX_TAREFA_STATUS_ID_STATUS",
                table: "TAREFA",
                column: "STATUS_ID_STATUS");

            migrationBuilder.CreateIndex(
                name: "IX_TAREFA_USUARIO_ID_USUARIO",
                table: "TAREFA",
                column: "USUARIO_ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "IX_TRILHA_PET_ID_PET",
                table: "TRILHA",
                column: "PET_ID_PET");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_EMAIL",
                table: "USUARIO",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_ENDERECO_ID_ENDERECO",
                table: "USUARIO",
                column: "ENDERECO_ID_ENDERECO");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_TELEFONE_ID_TELEFONE",
                table: "USUARIO",
                column: "TELEFONE_ID_TELEFONE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_PET_PET_ID_PET",
                table: "USUARIO_PET",
                column: "PET_ID_PET");

            migrationBuilder.Sql(@"
BEGIN
  INSERT INTO ""STATUS"" (""ID_STATUS"", ""NOME_STATUS"") VALUES (SYS_GUID(), 'PENDENTE');
  INSERT INTO ""STATUS"" (""ID_STATUS"", ""NOME_STATUS"") VALUES (SYS_GUID(), 'CONCLUIDO');
  INSERT INTO ""STATUS"" (""ID_STATUS"", ""NOME_STATUS"") VALUES (SYS_GUID(), 'EXPIRADO');
  INSERT INTO ""RACA"" (""ID_RACA"", ""NOME_RACA"") VALUES (SYS_GUID(), 'Labrador');
  INSERT INTO ""RACA"" (""ID_RACA"", ""NOME_RACA"") VALUES (SYS_GUID(), 'SRD');
  COMMIT;
END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AULA");

            migrationBuilder.DropTable(
                name: "HISTORICO");

            migrationBuilder.DropTable(
                name: "TAREFA");

            migrationBuilder.DropTable(
                name: "USUARIO_PET");

            migrationBuilder.DropTable(
                name: "MODULO");

            migrationBuilder.DropTable(
                name: "STATUS");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "TRILHA");

            migrationBuilder.DropTable(
                name: "ENDERECO");

            migrationBuilder.DropTable(
                name: "TELEFONE");

            migrationBuilder.DropTable(
                name: "PET");

            migrationBuilder.DropTable(
                name: "BAIRRO");

            migrationBuilder.DropTable(
                name: "RACA");

            migrationBuilder.DropTable(
                name: "CIDADE");

            migrationBuilder.DropTable(
                name: "ESTADO");
        }
    }
}
