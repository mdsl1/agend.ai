# Agend.AI Profissional — aplicativo mobile

## O que já funciona neste primeiro fluxo

1. O login envia `clinicaUuid`, e-mail e senha para `POST /api/auth/login`.
2. O access token retornado é guardado pelo Expo SecureStore.
3. O app chama `GET /api/me` para validar a sessão e carregar o perfil e as permissões.
4. Ao reabrir o app, o token salvo é validado novamente por `GET /api/me`.
5. Se o token expirou e a API responde `401`, o token local é apagado e o app volta ao login.
6. O botão “Sair da conta” apaga a sessão local. A API ainda não tem uma rota de logout.

O aplicativo não lê nem decodifica o JWT. A API valida o token e devolve o perfil por `GET /api/me`.

## Arquivos principais

```text
mobile/
├─ app/
│  ├─ _layout.tsx                 # restaura sessão e protege a navegação
│  ├─ index.tsx                   # tela inicial após autenticação
│  └─ (public)/login.tsx           # formulário de login
├─ src/
│  ├─ features/auth/
│  │  ├─ AuthProvider.tsx          # estado de autenticação compartilhado
│  │  ├─ authApi.ts                # contratos HTTP de login e perfil
│  │  └─ authTypes.ts              # tipos das respostas da API
│  └─ services/
│     ├─ api.ts                    # cliente HTTP comum do app
│     └─ session.ts                # grava, lê e apaga o token seguro
├─ .env.example                    # modelo de configuração local
├─ app.json                        # configuração do Expo
├─ package.json                    # dependências e comandos
└─ tsconfig.json                   # configuração do TypeScript
```

## Preparação local

1. Na raiz do repositório, inicie a API e o banco:

   ```powershell
   docker compose up --build db api
   ```

2. Em outro terminal, dentro desta pasta, crie sua configuração local a partir do modelo:

   ```powershell
   Copy-Item .env.example .env.local
   ```

3. Edite `.env.local` e preencha:

   - `EXPO_PUBLIC_API_URL`: endereço da API acessível pelo dispositivo.

   Exemplos de endereço da API:

   - Android Emulator: `http://10.0.2.2:5000`
   - Emulador iOS: `http://localhost:5000`
   - Celular físico: `http://IP-DO-COMPUTADOR:5000`, com celular e computador na mesma rede Wi-Fi.

   O UUID da clínica será informado na primeira tela do aplicativo e armazenado no SecureStore. Não coloque senhas, JWTs ou outras credenciais no arquivo de ambiente. `.env.local` está no `.gitignore`.

4. Instale as dependências e inicie o Expo:

   ```powershell
   npm install
   npx expo start
   ```

5. Abra o app no emulador ou escaneie o QR Code com o Expo Go. Se alterar `.env.local` enquanto o Expo estiver aberto, reinicie o servidor Expo.

## Como o código é dividido

- A tela de login chama `AuthProvider.signIn()`; ela não monta a URL nem envia `fetch` diretamente.
- `authApi.ts` conhece as rotas de autenticação: `POST /api/auth/login` e `GET /api/me`.
- `api.ts` executa as requisições. Para rotas protegidas, lê o token do SecureStore e envia `Authorization: Bearer ...`.
- `session.ts` é a única camada que acessa diretamente o SecureStore.
- A tela inicial solicita o UUID da clínica; ele é guardado no SecureStore e enviado no corpo do login. A tela de login oferece a opção de trocar de clínica, que apaga o UUID e o token salvos.
- `AuthProvider.tsx` coordena login, restauração, perfil carregado e saída da conta.
- `_layout.tsx` mostra uma tela de carregamento durante a restauração e impede abrir a área autenticada sem uma sessão válida.
- `index.tsx` demonstra o perfil recebido da API e a ação de logout local.

Quando forem implementadas as demais telas, cada funcionalidade terá seu serviço próprio, por exemplo `features/agenda/agendaApi.ts`. Esse serviço usará `apiRequest()` de `services/api.ts`, mantendo autenticação, base URL e tratamento de erros consistentes.
