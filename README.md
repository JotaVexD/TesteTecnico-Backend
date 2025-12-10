## Início Rápido

### Pré-requisitos
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Passos para Executar
1. Clone o repositório ou baixe os arquivos
2. Abra o terminal na pasta do backend:
3. Execute a aplicação:
```bash
dotnet run
```

4. Acesse os endpoints:
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

5. Baixe o Frontend do Teste Técnico:
- [Teste Técnico - Frontend](https://github.com/JotaVexD/TesteTenico-Frontend)

## Endpoints da API

**GET** `/api/repositories/search`
- Busca repositórios no GitHub

**POST** `/api/repositories/toggle-favorite`
- Adiciona/remove dos favoritos

**GET** `/api/repositories/relevant`
- Lista repositórios por relevância

## Executando Testes

```bash
# Navegue até a pasta de testes
cd backendAPI/src/RepositorioApi.Tests

# Execute todos os testes
dotnet test

# Executar com detalhes
dotnet test --logger "console;verbosity=normal"
```

## Configuração
A API está configurada para:

Porta: 5000 (HTTP) e 5001 (HTTPS)

CORS habilitado para http://localhost:4200
