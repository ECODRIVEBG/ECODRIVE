CREATE DATABASE IF NOT EXISTS EcoDrive;
USE EcoDrive;


CREATE TABLE Usuario (
    IdUsuario INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50) NOT NULL,
    Email VARCHAR(50) NOT NULL UNIQUE,
    Senha VARCHAR(50) NOT NULL,
    CPF VARCHAR(11) NOT NULL UNIQUE
);


CREATE TABLE Cliente (
    IdCliente INT PRIMARY KEY,
    NomeSocial VARCHAR(50) NULL,
    Bloqueado BOOLEAN NOT NULL DEFAULT FALSE,
    Saldo FLOAT(10,2) NOT NULL DEFAULT 0.00,
    FOREIGN KEY (IdCliente) REFERENCES Usuario(IdUsuario)
);


CREATE TABLE Funcionario (
    IdFuncionario INT PRIMARY KEY,
    Cargo VARCHAR(50) NOT NULL,
    Telefone VARCHAR(30) NULL,
    NivelAcesso VARCHAR(20) NOT NULL DEFAULT 'Funcionario',
    FOREIGN KEY (IdFuncionario) REFERENCES Usuario(IdUsuario)
);

CREATE TABLE Ponto (
    IdPonto INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(80) NOT NULL,
    Endereco VARCHAR(120) NOT NULL,
    Bairro VARCHAR(50) NOT NULL,
    Tipo VARCHAR(30) NOT NULL DEFAULT 'Estacao',
    Ativo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE Veiculo (
    IdVeiculo INT AUTO_INCREMENT PRIMARY KEY,
    IdPonto INT NOT NULL,
    Tipo VARCHAR(30) NOT NULL,
    Status_ VARCHAR(30) NOT NULL DEFAULT 'Disponivel',
    NivelBateria INT NULL CHECK (NivelBateria BETWEEN 0 AND 100),
    Chave VARCHAR(50) NOT NULL,
    FOREIGN KEY (IdPonto) REFERENCES Ponto(IdPonto)
);


CREATE TABLE Historico_loc (
    IdHistLoc INT AUTO_INCREMENT PRIMARY KEY,
    IdCliente INT NOT NULL,
    IdVeiculo INT NOT NULL,
    IdPontoInicio INT NOT NULL,
    IdPontoFim INT NULL,
    InicioCorrida DATETIME NOT NULL,
    FimCorrida DATETIME NULL,
    ValorCorrida FLOAT(10,2) NULL,
    Problema BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente),
    FOREIGN KEY (IdVeiculo) REFERENCES Veiculo(IdVeiculo),
    FOREIGN KEY (IdPontoInicio) REFERENCES Ponto(IdPonto),
    FOREIGN KEY (IdPontoFim) REFERENCES Ponto(IdPonto)
);


CREATE TABLE HistPgto (
    IdHistPgto INT AUTO_INCREMENT PRIMARY KEY,
    IdCliente INT NOT NULL,
    ValorRecarga FLOAT(10,2) NOT NULL,
    DataRecarga DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente)
);


CREATE TABLE Manutencao (
    IdManutencao INT AUTO_INCREMENT PRIMARY KEY,
    IdVeiculo INT NOT NULL,
    IdFuncionario INT NOT NULL,
    DataInicio DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DataFim DATETIME NULL,
    Descricao VARCHAR(255) NULL,
    Concluida BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (IdVeiculo) REFERENCES Veiculo(IdVeiculo),
    FOREIGN KEY (IdFuncionario) REFERENCES Funcionario(IdFuncionario)
);


INSERT INTO Ponto (Nome, Endereco, Bairro, Tipo, Ativo) VALUES
('Estação Metrô Pinheiros', 'Rua Gilberto Sabino, 169 - Pinheiros', 'Pinheiros', 'Estacao', TRUE),
('Estação Faria Lima', 'Avenida Brigadeiro Faria Lima, 940 - Pinheiros', 'Pinheiros', 'Estacao', TRUE),
('Estação Fradique Coutinho', 'Rua dos Pinheiros, 623 - Pinheiros', 'Pinheiros', 'Estacao', TRUE),
('Largo da Batata', 'Avenida Brigadeiro Faria Lima - Pinheiros', 'Pinheiros', 'Estacao', TRUE),
('Sesc Pinheiros', 'Rua Paes Leme, 195 - Pinheiros', 'Pinheiros', 'Estacao', TRUE),
('Instituto Tomie Ohtake', 'Rua Coropés, 88 - Pinheiros', 'Pinheiros', 'Estacao', TRUE),
('Praça Benedito Calixto', 'Praça Benedito Calixto - Pinheiros', 'Pinheiros', 'Estacao', TRUE),
('Mercado Municipal de Pinheiros', 'Rua Pedro Cristi, 89 - Pinheiros', 'Pinheiros', 'Estacao', TRUE),
('Shopping Eldorado', 'Avenida Rebouças, 3970 - Pinheiros', 'Pinheiros', 'Estacao', TRUE),
('Hospital das Clínicas FMUSP', 'Av. Doutor Enéas Carvalho de Aguiar, 255 - Cerqueira César', 'Pinheiros', 'Estacao', TRUE),
('Brascan Century Plaza', 'Rua Joaquim Floriano, 466 - Itaim Bibi', 'Itaim Bibi', 'Estacao', TRUE),
('Eataly São Paulo', 'Avenida Presidente Juscelino Kubitschek, 1489 - Itaim Bibi', 'Itaim Bibi', 'Estacao', TRUE),
('Edifício Pátio Victor Malzoni', 'Avenida Brigadeiro Faria Lima, 3477 - Itaim Bibi', 'Itaim Bibi', 'Estacao', TRUE),
('Praça da Baleia', 'Avenida Brigadeiro Faria Lima, 3732 - Itaim Bibi', 'Itaim Bibi', 'Estacao', TRUE),
('Parque do Povo', 'Avenida Henrique Chamma, 420 - Itaim Bibi', 'Itaim Bibi', 'Estacao', TRUE),
('Museu da Casa Brasileira', 'Avenida Brigadeiro Faria Lima, 2705 - Itaim Bibi', 'Itaim Bibi', 'Estacao', TRUE),
('Shopping JK Iguatemi', 'Avenida Presidente Juscelino Kubitschek, 2041 - Itaim Bibi', 'Itaim Bibi', 'Estacao', TRUE),
('São Paulo Corporate Towers', 'Av. Presidente Juscelino Kubitschek, 1909 - Itaim Bibi', 'Itaim Bibi', 'Estacao', TRUE),
('Hospital São Luiz Itaim', 'Rua Doutor Alceu de Campos Rodrigues, 95 - Itaim Bibi', 'Itaim Bibi', 'Estacao', TRUE),
('Polo Comercial João Cachoeira', 'Rua João Cachoeira, 914 - Itaim Bibi', 'Itaim Bibi', 'Estacao', TRUE),
('Estação Vila Madalena', 'Rua Doutor Paulo Vieira, 10 - Sumarezinho', 'Vila Madalena', 'Estacao', TRUE),
('Beco do Batman', 'Rua Medeiros de Albuquerque, 82 - Jardim das Bandeiras', 'Vila Madalena', 'Estacao', TRUE),
('Livraria da Vila', 'Rua Fradique Coutinho, 915 - Vila Madalena', 'Vila Madalena', 'Estacao', TRUE),
('Coffee Lab', 'Rua Aspicuelta, 227 - Vila Madalena', 'Vila Madalena', 'Estacao', TRUE),
('Bar Astor', 'Rua Delfina, 163 - Vila Madalena', 'Vila Madalena', 'Estacao', TRUE),
('Parque Linear das Corujas', 'Rua Natingui - Vila Madalena', 'Vila Madalena', 'Estacao', TRUE),
('Beco do Aprendiz', 'Rua Belmiro Braga, 113 - Vila Madalena', 'Vila Madalena', 'Estacao', TRUE),
('Peixaria Bar e Venda', 'Rua Inácio Pereira da Rocha, 112 - Vila Madalena', 'Vila Madalena', 'Estacao', TRUE),
('Armazém Piola', 'Rua Aspicuelta, 547 - Vila Madalena', 'Vila Madalena', 'Estacao', TRUE),
('Pão de Açúcar', 'Rua Heitor Penteado, 250 - Sumarezinho', 'Vila Madalena', 'Estacao', TRUE);