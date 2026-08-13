CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS clinicas (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    cnpj VARCHAR(18) UNIQUE,
    nome VARCHAR(150) NOT NULL,
    telefone VARCHAR(20),
    endereco TEXT,
    tipo_clinica VARCHAR(50) DEFAULT 'medica', -- 'medica', 'estetica', 'odontologica'
    webhook_calendar TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS horario_funcionamento (
    id BIGSERIAL PRIMARY KEY,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    dia_semana SMALLINT NOT NULL CHECK (dia_semana BETWEEN 0 AND 6), -- 0=Dom, 1=Seg... 6=Sáb
    hora_inicio TIME NOT NULL,
    hora_fim TIME NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_clinica_dia UNIQUE (id_clinica, dia_semana)
);

CREATE TABLE IF NOT EXISTS especialidades (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    nome VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS usuarios (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    nome VARCHAR(150) NOT NULL,
    email VARCHAR(150) NOT NULL,
    senha_hash VARCHAR(255) NOT NULL,
    cargo VARCHAR(50) NOT NULL, -- 'Profissional', 'Recepcionista', 'Administrador'
    is_admin BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT uq_usuario_email_clinica UNIQUE (id_clinica, email)
);

CREATE TABLE IF NOT EXISTS procedimentos (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    nome VARCHAR(100) NOT NULL,
    duracao_estimada_minutos INT NOT NULL DEFAULT 30,
    valor_base NUMERIC(10, 2) NOT NULL DEFAULT 0.00,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS profissionais (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_usuario BIGINT NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    id_especialidade BIGINT REFERENCES especialidades(id) ON DELETE SET NULL,
    registro_profissional VARCHAR(30), -- CRM, CRO, etc.
    id_google_calendar TEXT,
    prefixo VARCHAR(6),
    CONSTRAINT uq_profissional_usuario UNIQUE (id_usuario)
);

CREATE TABLE IF NOT EXISTS clientes (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    nome VARCHAR(150) NOT NULL,
    cpf VARCHAR(14),
    email VARCHAR(150),
    telefone VARCHAR(20) NOT NULL,
    data_nascimento DATE,
    genero VARCHAR(20),
    observacoes_anamnese TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS agendamentos (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    id_cliente BIGINT NOT NULL REFERENCES clientes(id) ON DELETE RESTRICT,
    id_profissional BIGINT NOT NULL REFERENCES profissionais(id) ON DELETE RESTRICT,
    id_especialidade BIGINT REFERENCES especialidades(id) ON DELETE SET NULL,
    id_procedimento BIGINT REFERENCES procedimentos(id) ON DELETE SET NULL,
    id_event_google_calendar TEXT,
    timedate_inicio TIMESTAMPTZ NOT NULL,
    timedate_fim TIMESTAMPTZ NOT NULL,
    motivo_contato TEXT,
    anotacoes_profissional TEXT,
    valor_total NUMERIC(10, 2) NOT NULL DEFAULT 0.00,
    status_pagamento VARCHAR(20) NOT NULL DEFAULT 'pendente', -- 'pendente', 'pago', 'isento'
    status VARCHAR(20) NOT NULL DEFAULT 'pendente_integracao', -- 'pendente_integracao', 'agendado', 'falha_integracao', 'concluido', 'cancelado', 'faltou'
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

-- Mantém volumes locais já existentes compatíveis com a evolução do schema.
ALTER TABLE profissionais
    ADD COLUMN IF NOT EXISTS id_google_calendar TEXT,
    ADD COLUMN IF NOT EXISTS prefixo VARCHAR(6);

ALTER TABLE profissionais
    DROP CONSTRAINT IF EXISTS uq_profissional_usuario_id_calendar;

CREATE UNIQUE INDEX IF NOT EXISTS uq_profissional_usuario
ON profissionais(id_usuario);

CREATE UNIQUE INDEX IF NOT EXISTS uq_profissional_google_calendar
ON profissionais(id_google_calendar)
WHERE id_google_calendar IS NOT NULL;

ALTER TABLE agendamentos
    ADD COLUMN IF NOT EXISTS id_event_google_calendar TEXT;

CREATE UNIQUE INDEX IF NOT EXISTS uq_agendamento_google_event
ON agendamentos(id_event_google_calendar)
WHERE id_event_google_calendar IS NOT NULL;

ALTER TABLE agendamentos
    ALTER COLUMN status SET DEFAULT 'pendente_integracao';


CREATE INDEX idx_agendamento_grid ON agendamentos(id_clinica, id_profissional, timedate_inicio, timedate_fim) 
WHERE deleted_at IS NULL;

CREATE INDEX idx_agendamento_cliente ON agendamentos(id_cliente) 
WHERE deleted_at IS NULL;

CREATE INDEX idx_clientes_busca ON clientes(id_clinica, nome, telefone, cpf) 
WHERE deleted_at IS NULL;

CREATE INDEX idx_usuarios_login ON usuarios(email, id_clinica) 
WHERE deleted_at IS NULL;


INSERT INTO clinicas (id, nome, cnpj, tipo_clinica, webhook_calendar) 
VALUES (1, 'Clínica Agend.AI Central', '12.345.678/0001-90', 'medica', 'https://n8n.mdsl1.com/webhook/8c6ff736-f915-4a03-9a44-5292a7f61fea')
ON CONFLICT DO NOTHING;

INSERT INTO horario_funcionamento (id_clinica, dia_semana, hora_inicio, hora_fim) VALUES
(1, 1, '08:00', '18:00'), -- Segunda
(1, 2, '08:00', '18:00'), -- Terça
(1, 3, '08:00', '18:00'), -- Quarta
(1, 4, '08:00', '18:00'), -- Quinta
(1, 5, '08:00', '18:00'), -- Sexta
(1, 6, '08:00', '12:00')  -- Sábado
ON CONFLICT DO NOTHING;

INSERT INTO especialidades (id, id_clinica, nome) VALUES
(1, 1, 'Clínica Geral'),
(2, 1, 'Dermatologia'),
(3, 1, 'Cardiologia')
ON CONFLICT DO NOTHING;
