CREATE DATABASE IF NOT EXISTS EcoDrive;
USE EcoDrive;

-- Usuario: tabela base da herança (comum a Cliente e Funcionario)
CREATE TABLE Usuario (
    IdUsuario INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50) NOT NULL,
    Email VARCHAR(50) NOT NULL UNIQUE,
    Senha VARCHAR(50) NOT NULL,
    CPF VARCHAR(11) NOT NULL UNIQUE
);

-- Cliente: especialização de Usuario
CREATE TABLE Cliente (
    IdCliente INT PRIMARY KEY,
    NomeSocial VARCHAR(50) NULL,
    Bloqueado BOOLEAN NOT NULL DEFAULT FALSE,
    Saldo FLOAT(10,2) NOT NULL DEFAULT 0.00,
    FOREIGN KEY (IdCliente) REFERENCES Usuario(IdUsuario)
);

-- Funcionario: especialização de Usuario (Admin = Funcionario com NivelAcesso)
CREATE TABLE Funcionario (
    IdFuncionario INT PRIMARY KEY,
    Cargo VARCHAR(50) NOT NULL,
    Telefone VARCHAR(30) NULL,
    NivelAcesso VARCHAR(20) NOT NULL DEFAULT 'Funcionario',
    FOREIGN KEY (IdFuncionario) REFERENCES Usuario(IdUsuario)
);