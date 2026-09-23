CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS btree_gist;

CREATE TABLE IF NOT EXISTS clinicas (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    cnpj VARCHAR(18),
    nome VARCHAR(150) NOT NULL,
    telefone VARCHAR(20),
    endereco TEXT,
    tipo_clinica VARCHAR(50) NOT NULL DEFAULT 'medica', -- 'medica', 'estetica', 'odontologica'
    webhook_calendar TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT ck_clinica_tipo
        CHECK (tipo_clinica IN ('medica', 'estetica', 'odontologica')),
    CONSTRAINT ck_clinica_nome_nao_vazio
        CHECK (btrim(nome) <> ''),
    CONSTRAINT ck_clinica_cnpj_valido
        CHECK (
            cnpj IS NULL
            OR char_length(regexp_replace(cnpj, '[^0-9]', '', 'g')) = 14
        ),
    CONSTRAINT ck_clinica_webhook_calendar_nao_vazio
        CHECK (webhook_calendar IS NULL OR btrim(webhook_calendar) <> '')
);

CREATE TABLE IF NOT EXISTS horario_funcionamento (
    id BIGSERIAL PRIMARY KEY,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE RESTRICT,
    dia_semana SMALLINT NOT NULL CHECK (dia_semana BETWEEN 0 AND 6), -- 0=Dom, 1=Seg... 6=Sáb
    hora_inicio TIME NOT NULL,
    hora_fim TIME NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT ck_horario_funcionamento_periodo
        CHECK (hora_inicio < hora_fim),
    CONSTRAINT uq_clinica_dia UNIQUE (id_clinica, dia_semana)
);

CREATE TABLE IF NOT EXISTS especialidades (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE RESTRICT,
    nome VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT ck_especialidade_nome_nao_vazio
        CHECK (btrim(nome) <> ''),
    CONSTRAINT uq_especialidade_clinica_id UNIQUE (id_clinica, id)
);

CREATE TABLE IF NOT EXISTS usuarios (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE RESTRICT,
    nome VARCHAR(150) NOT NULL,
    email VARCHAR(150) NOT NULL,
    senha_hash VARCHAR(255) NOT NULL,
    cargo VARCHAR(50) NOT NULL, -- 'Profissional', 'Recepcionista', 'Administrador'
    is_admin BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT ck_usuario_cargo
        CHECK (cargo IN ('Profissional', 'Recepcionista', 'Administrador')),
    CONSTRAINT ck_usuario_nome_nao_vazio
        CHECK (btrim(nome) <> ''),
    CONSTRAINT ck_usuario_email_nao_vazio
        CHECK (btrim(email) <> ''),
    CONSTRAINT ck_usuario_senha_hash_nao_vazio
        CHECK (btrim(senha_hash) <> ''),
    CONSTRAINT uq_usuario_clinica_id UNIQUE (id_clinica, id)
);

CREATE TABLE IF NOT EXISTS procedimentos (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE RESTRICT,
    nome VARCHAR(100) NOT NULL,
    duracao_estimada_minutos INT NOT NULL DEFAULT 30,
    valor_base NUMERIC(10, 2) NOT NULL DEFAULT 0.00,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT ck_procedimento_nome_nao_vazio
        CHECK (btrim(nome) <> ''),
    CONSTRAINT ck_procedimento_duracao_positiva
        CHECK (duracao_estimada_minutos > 0),
    CONSTRAINT ck_procedimento_valor_nao_negativo
        CHECK (valor_base >= 0),
    CONSTRAINT uq_procedimento_clinica_id UNIQUE (id_clinica, id)
);

CREATE TABLE IF NOT EXISTS profissionais (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE RESTRICT,
    id_usuario BIGINT NOT NULL,
    id_especialidade BIGINT NOT NULL,
    registro_profissional VARCHAR(30), -- CRM, CRO, etc.
    id_google_calendar TEXT,
    prefixo VARCHAR(6),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT ck_profissional_registro_nao_vazio
        CHECK (registro_profissional IS NULL OR btrim(registro_profissional) <> ''),
    CONSTRAINT ck_profissional_google_calendar_nao_vazio
        CHECK (id_google_calendar IS NULL OR btrim(id_google_calendar) <> ''),
    CONSTRAINT ck_profissional_prefixo_nao_vazio
        CHECK (prefixo IS NULL OR btrim(prefixo) <> ''),
    CONSTRAINT fk_profissional_usuario_clinica
        FOREIGN KEY (id_clinica, id_usuario)
        REFERENCES usuarios(id_clinica, id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_profissional_especialidade_clinica
        FOREIGN KEY (id_clinica, id_especialidade)
        REFERENCES especialidades(id_clinica, id)
        ON DELETE RESTRICT,
    CONSTRAINT uq_profissional_usuario UNIQUE (id_usuario),
    CONSTRAINT uq_profissional_clinica_id UNIQUE (id_clinica, id)
);

CREATE TABLE IF NOT EXISTS profissional_procedimentos(
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE RESTRICT,
    id_profissional BIGINT NOT NULL,
    id_procedimento BIGINT NOT NULL,
    valor NUMERIC(10, 2) NOT NULL,
    duracao_minutos INT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT ck_profissional_procedimento_duracao_positiva
        CHECK (duracao_minutos > 0),
    CONSTRAINT ck_profissional_procedimento_valor_nao_negativo
        CHECK (valor >= 0),
    CONSTRAINT uq_profissional_procedimento
        UNIQUE (id_profissional, id_procedimento),
    CONSTRAINT uq_profissional_procedimento_clinica_id UNIQUE (id_clinica, id),
    CONSTRAINT fk_profissional_procedimento_profissional_clinica
        FOREIGN KEY (id_clinica, id_profissional)
        REFERENCES profissionais(id_clinica, id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_profissional_procedimento_procedimento_clinica
        FOREIGN KEY (id_clinica, id_procedimento)
        REFERENCES procedimentos(id_clinica, id)
        ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS clientes (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE RESTRICT,
    nome VARCHAR(150) NOT NULL,
    cpf VARCHAR(14),
    email VARCHAR(150),
    id_telegram TEXT,
    telefone VARCHAR(20) NOT NULL,
    data_nascimento DATE,
    genero VARCHAR(20),
    observacoes_anamnese TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT ck_cliente_telefone_valido
        CHECK (
            char_length(regexp_replace(telefone, '[^0-9]', '', 'g'))
            BETWEEN 10 AND 15
        ),
    CONSTRAINT ck_cliente_nome_nao_vazio
        CHECK (btrim(nome) <> ''),
    CONSTRAINT ck_cliente_cpf_valido
        CHECK (
            cpf IS NULL
            OR char_length(regexp_replace(cpf, '[^0-9]', '', 'g')) = 11
        ),
    CONSTRAINT ck_cliente_email_nao_vazio
        CHECK (email IS NULL OR btrim(email) <> ''),
    CONSTRAINT ck_cliente_telegram_nao_vazio
        CHECK (id_telegram IS NULL OR btrim(id_telegram) <> ''),
    CONSTRAINT uq_cliente_clinica_id UNIQUE (id_clinica, id)
);

CREATE TABLE IF NOT EXISTS agendamentos (
    id BIGSERIAL PRIMARY KEY,
    uuid UUID NOT NULL DEFAULT uuid_generate_v4() UNIQUE,
    chave_idempotencia VARCHAR(200) NOT NULL,
    id_clinica BIGINT NOT NULL REFERENCES clinicas(id) ON DELETE RESTRICT,
    id_cliente BIGINT NOT NULL,
    id_profissional BIGINT NOT NULL,
    id_especialidade BIGINT NOT NULL,
    id_procedimento BIGINT NOT NULL,
    id_event_google_calendar TEXT,
    timedate_inicio TIMESTAMPTZ NOT NULL,
    timedate_fim TIMESTAMPTZ NOT NULL,
    motivo_contato TEXT,
    anotacoes_profissional TEXT,
    valor_total NUMERIC(10, 2) NOT NULL,
    status_pagamento VARCHAR(20) NOT NULL DEFAULT 'pendente', -- 'pendente', 'pago', 'isento'
    status VARCHAR(20) NOT NULL DEFAULT 'pendente_integracao', -- 'pendente_integracao', 'agendado', 'falha_integracao', 'concluido', 'cancelado', 'faltou'
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT uq_agendamento_clinica_chave_idempotencia 
        UNIQUE (id_clinica, chave_idempotencia),
    CONSTRAINT ck_agendamento_periodo_valido
        CHECK (timedate_inicio < timedate_fim),
    CONSTRAINT ck_agendamento_valor_nao_negativo
        CHECK (valor_total >= 0),
    CONSTRAINT ck_agendamento_google_event_nao_vazio
        CHECK (id_event_google_calendar IS NULL OR btrim(id_event_google_calendar) <> ''),
    CONSTRAINT ck_agendamento_status_pagamento
        CHECK (status_pagamento IN ('pendente', 'pago', 'isento')),
    CONSTRAINT ck_agendamento_status
        CHECK (
            status IN (
                'pendente_integracao',
                'agendado',
                'falha_integracao',
                'concluido',
                'cancelado',
                'faltou'
            )
        ),
    CONSTRAINT fk_agendamento_cliente_clinica
        FOREIGN KEY (id_clinica, id_cliente)
        REFERENCES clientes(id_clinica, id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_agendamento_profissional_clinica
        FOREIGN KEY (id_clinica, id_profissional)
        REFERENCES profissionais(id_clinica, id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_agendamento_especialidade_clinica
        FOREIGN KEY (id_clinica, id_especialidade)
        REFERENCES especialidades(id_clinica, id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_agendamento_procedimento_clinica
        FOREIGN KEY (id_clinica, id_procedimento)
        REFERENCES procedimentos(id_clinica, id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_agendamento_profissional_procedimento
        FOREIGN KEY (id_profissional, id_procedimento)
        REFERENCES profissional_procedimentos(id_profissional, id_procedimento)
        ON DELETE RESTRICT,
    CONSTRAINT ex_agendamento_sem_sobreposicao
        EXCLUDE USING gist (
            id_profissional WITH =,
            tstzrange(timedate_inicio, timedate_fim, '[)') WITH &&
        )
        WHERE (
            deleted_at IS NULL
            AND status IN ('pendente_integracao', 'agendado')
        )
);


CREATE UNIQUE INDEX IF NOT EXISTS uq_clinica_cnpj
ON clinicas (
    regexp_replace(cnpj, '[^0-9]', '', 'g')
)
WHERE cnpj IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS uq_especialidade_nome_ativo
ON especialidades (
    id_clinica,
    lower(btrim(nome))
)
WHERE deleted_at IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS uq_usuario_email_clinica
ON usuarios (
    id_clinica,
    lower(btrim(email))
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_procedimento_nome_ativo
ON procedimentos (
    id_clinica,
    lower(btrim(nome))
)
WHERE deleted_at IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS uq_profissional_google_calendar
ON profissionais(id_google_calendar)
WHERE id_google_calendar IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS uq_agendamento_google_event
ON agendamentos(id_event_google_calendar)
WHERE id_event_google_calendar IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS uq_cliente_telefone_ativo
ON clientes (
    id_clinica,
    regexp_replace(telefone, '[^0-9]', '', 'g')
)
WHERE deleted_at IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS uq_cliente_telegram_ativo
ON clientes (
    id_clinica,
    id_telegram
)
WHERE deleted_at IS NULL
AND id_telegram IS NOT NULL;


CREATE INDEX IF NOT EXISTS idx_profissionais_especialidade
ON profissionais(id_clinica, id_especialidade);

CREATE INDEX IF NOT EXISTS idx_agendamento_grid
ON agendamentos(id_clinica, id_profissional, timedate_inicio, timedate_fim)
WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS idx_agendamento_cliente
ON agendamentos(id_clinica, id_cliente);

CREATE INDEX IF NOT EXISTS idx_agendamento_especialidade
ON agendamentos(id_clinica, id_especialidade);

CREATE INDEX IF NOT EXISTS idx_agendamento_procedimento
ON agendamentos(id_clinica, id_procedimento);

CREATE INDEX IF NOT EXISTS idx_agendamento_profissional_procedimento
ON agendamentos(id_profissional, id_procedimento);

CREATE INDEX IF NOT EXISTS idx_clientes_nome_ativo
ON clientes(id_clinica, lower(btrim(nome)))
WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS idx_clientes_cpf_ativo
ON clientes (
    id_clinica,
    regexp_replace(cpf, '[^0-9]', '', 'g')
)
WHERE deleted_at IS NULL
AND cpf IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_profissional_procedimentos_profissional
ON profissional_procedimentos(id_clinica, id_profissional);

CREATE INDEX IF NOT EXISTS idx_profissional_procedimentos_procedimento
ON profissional_procedimentos(id_clinica, id_procedimento);


INSERT INTO clinicas (nome, cnpj, tipo_clinica)
VALUES ('Clínica Vitality', '12.345.678/0001-95', 'medica')
ON CONFLICT DO NOTHING;

INSERT INTO horario_funcionamento (id_clinica, dia_semana, hora_inicio, hora_fim) VALUES
(1, 1, '08:00', '18:00'), -- Segunda
(1, 2, '08:00', '18:00'), -- Terça
(1, 3, '08:00', '18:00'), -- Quarta
(1, 4, '08:00', '18:00'), -- Quinta
(1, 5, '08:00', '18:00'), -- Sexta
(1, 6, '08:00', '12:00')  -- Sábado
ON CONFLICT DO NOTHING;

INSERT INTO especialidades (id_clinica, nome) VALUES
(1, 'Clínica Geral'),
(1, 'Dermatologia'),
(1, 'Cardiologia')
ON CONFLICT DO NOTHING;
