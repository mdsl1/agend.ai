# Diretrizes do agente para o Agend.AI Profissional

Estas regras se aplicam a qualquer análise, implementação ou revisão do código dentro de `mobile/`. Leia este arquivo antes de trabalhar no app. As instruções do usuário na conversa atual continuam tendo precedência.

## 1. Fontes obrigatórias antes de integrar com a API

Antes de alterar qualquer código mobile que consuma, altere ou dependa de comportamento da API, consulte:

1. `docs/Rotas da API RESTful — MVP Acadêmico Web, Mobile e Chatbot.md` — contrato versionado e fonte canônica das rotas;
2. `C:\Users\markn\mdsl1\Projetos Pessoais\Agend.AI\Projeto Mobile (Querino)\Relatório - Projeto de Laboratório.md` — escopo e decisões do MVP mobile;
3. `AGENTS.md` da raiz deste repositório — arquitetura, estado implementado e regras gerais;
4. o código efetivo da rota na API quando a implementação puder divergir do contrato.

Não deduza nomes de campos, permissões, status, formatos de data, erros ou fluxos que não estejam claros nessas fontes. Se houver lacuna, contradição ou fonte necessária indisponível, pare a parte afetada e pergunte ao usuário antes de escolher um comportamento. Preserve decisões explícitas mais recentes do usuário.

## 2. Sincronização da documentação e dos estados

Ao concluir uma alteração de tela ou fluxo mobile, atualize:

- o estado da tela/fluxo no relatório do projeto;
- a cobertura de telas/fluxos mobile e suas rotas no documento canônico de rotas;
- o registro de estado/estrutura mobile no `AGENTS.md` da raiz.

Mude **Planejado** para **Implementado** somente quando a tela estiver realmente concluída no escopo definido e passar pelos testes cabíveis. O fato de uma rota de API existir não significa que a integração mobile esteja implementada; e implementar uma tela não muda o estado de implementação do endpoint no backend. Se um desses documentos ainda não tiver campo de estado para tela, acrescente ou adapte um registro curto e explícito, sem alterar o significado dos estados das rotas da API. Atualize descrições e estado real; não marque trabalho incompleto como concluído.

Se o usuário não solicitar outra coisa, registre também quais verificações foram executadas e seus resultados, sem alegar teste visual no dispositivo quando ele não foi realizado.

## 3. Simplicidade, idioma e estrutura

- Use português em nomes novos de variáveis, funções, tipos, componentes, pastas e arquivos, mantendo em inglês apenas contratos externos, nomes exigidos por bibliotecas/Expo e convenções inevitáveis.
- Prefira a solução mais simples que seja funcional, segura, legível e fácil de depurar. Evite abstrações, dependências e camadas sem necessidade concreta.
- Organize cada tela em uma pasta com `index.tsx` e `styles.ts`, por exemplo `src/features/autenticacao/telas/Entrar/index.tsx` e `styles.ts`.
- Como o Expo Router descobre arquivos dentro de `app/` como rotas, mantenha nessa pasta apenas arquivos de rota reconhecíveis. Use o arquivo de rota como adaptador fino para a tela organizada em `src/`; não coloque `styles.ts` dentro de `app/` se isso fizer o Router tratá-lo como uma rota.
- Preserve caminhos de rota já consumidos por links/deep links, salvo se a mudança for necessária e compatível com o Expo Router; explique qualquer mudança de URL interna relevante.

## 4. Testes e validação

Ao final de cada alteração, execute os testes disponíveis e pertinentes, no mínimo:

```powershell
npm run typecheck
```

Execute também `npm run lint` se esse script existir; rode testes automatizados e verificações Expo aplicáveis ao escopo. Não declare sucesso para uma verificação que falhou ou não foi executada. Testes visuais no dispositivo/emulador podem ser feitos pelo usuário; informe claramente quando não tiverem sido executados.

## 5. Design e Figma

- Use `figma.css` como referência quando houver especificações aplicáveis; preserve a identidade visual, o espaçamento de borda, a hierarquia e alvos de toque confortáveis.
- O relatório mobile define orientação geral: frames de referência de 390 × 844, safe area, margem lateral aproximada de 20 pt e áreas de toque próximas de 44 × 44 pt.
- `figma.css` contém especificações de telas mobile exportadas do Figma, além de estilos desktop. Consulte os grupos `figma-mobile-mvp-screens__layer-*` e comentários com o nome da tela; use-os para identificar paleta, hierarquia e componentes, sem copiar posicionamentos percentuais absolutos do board como layout React Native. Se não houver referência para a tela ou componente em questão, avise o usuário e solicite o material antes de inventar a solução visual.
- Divergências pequenas necessárias à usabilidade mobile são aceitáveis, desde que preservem a essência do Figma. Informe divergências relevantes.

## 6. Boas práticas Expo e React Native

- Respeite Expo Router: layouts compõem navegação; telas ficam em rotas; não crie ciclos entre `Redirect`, estado de autenticação e montagem do navegador.
- Centralize chamadas HTTP em serviços; reutilize o cliente `apiRequest` e os contratos tipados existentes. Componentes não devem montar URLs arbitrárias nem duplicar autenticação.
- Mantenha JWT e dados sensíveis no SecureStore; nunca coloque segredos em variáveis `EXPO_PUBLIC_*`, no bundle, logs ou mensagens de erro.
- Trate estados de carregamento, vazio, erro, sessão expirada e falta de conexão quando relevantes. Preserve entrada do usuário em erros recuperáveis.
- Use `SafeAreaView`/safe area, ajuste ao teclado e rolagem quando necessário, controles acessíveis, feedback de toque e alvos de toque adequados.
- Evite efeitos colaterais durante renderização, efeitos com dependências instáveis e atualizações de estado que possam causar loops. Valide navegação e estados de autenticação depois de mexer no guard.
- Preserve a separação entre interface, lógica de tela, serviços e armazenamento existente, sem replicar regras de autorização que pertencem à API.
