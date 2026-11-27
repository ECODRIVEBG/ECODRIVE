Instruções para a execução do projeto:

No seu SGBD execute o SQL que se encontra no arquivo de texto schema.txt dentro da pasta DB
Antes de executar o projeto, abra o arquivo appsettings.json e coloque a sua senha.
Após rodar o Script do banco de dados, os usuários Colaborador e Administrador estarão cadastrados dentro do banco, seus emails e senhas:

Administrador
E-mail = admin@ecodrive.com
Senha = admin123

Colaborador
Email = colaborador@ecodrive.com
Senha = colab123

Além dos usuários, 3 bikes e 3 patinetes já estão cadastrados no banco:

Bikes
Token = 1001-B PontoP = Estação Metrô Pinheiros
Token = 1002-B  PontoP = Brascan Century Plaza 
Token = 1003-B  PontoP = Estação Vila Madalena 

Patinetes
Token = 2001-P  Ponto = Sesc Pinheiros
Token = 2002-P  Ponto = Eataly São Paulo
Token = 2003-P  Ponto = Livraria da Vila

É importante lembrar que um veiculo (Bike ou Patinete eletrico) só podem ser retirados de um ponto P se de fato eles estiverem lá
A principio a maioria dos pontos P não estão com veículos para serem retirados dele, por isso apenas nos 6 pontos P que guardam esses 6 veiculos é possivel iniciar uma corrida
Para alugar um ativo, o cliente escolhe o Ponto P, digita o código do ativo (ex.: 1001-B) e confirma a corrida.

Para alugar, o cliente precisa ter saldo (cada minuto custa R$ 1,00). Crie uma conta de cliente na tela de cadastro, entre e faça uma recarga na aba Saldo. A recarga é simulada, sem pagamento real, e o QR Code do Pix é só demonstrativo.


Obs: Isso pode ser facilmente alterado, por meio do Administrador que tem autorização para cadastrar novos veículos e mais pontosP
Atenção: Evite excluir o seu próprio perfil de administrador ou excluir um Funcionário que esta ligado a uma manutenção





