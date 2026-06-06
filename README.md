# MetanoBR

Sistema de monitoramento de vazamentos de metano (CH4), feito para a FIAP Global Solution 2025.

## Para que serve

O metano é um dos gases que mais contribuem para o aquecimento global. Em pouco tempo na atmosfera ele retém muito mais calor do que o gás carbônico, então cada vazamento evitado faz diferença grande para o clima.

O problema é que esses vazamentos costumam ser invisíveis e silenciosos. Eles acontecem em poços de petróleo e gás, em aterros sanitários, em dutos e na pecuária, e muitas vezes ninguém percebe até virar um problema sério. Quando o vazamento é descoberto cedo, dá para consertar rápido, com menos gás perdido, menos risco de explosão e menos dano ambiental.

O MetanoBR ajuda exatamente nessa parte: ele acompanha o ar em vários pontos ao mesmo tempo e avisa quando algo sai do normal. A ideia é simples: quanto antes o vazamento é detectado, antes ele pode ser fechado.

Na prática, o sistema é útil para:

- Encontrar vazamentos cedo, antes que fiquem grandes
- Vigiar áreas de risco sem precisar de uma pessoa olhando o tempo todo
- Mostrar onde e quando o metano subiu, para decidir o que fazer primeiro
- Reduzir gás desperdiçado, custo e impacto no clima

## Como funciona, em resumo

O sistema usa sensores espalhados pelo território (no chão, em rede IoT e em órbita) e também satélites que sobrevoam as regiões. A cada coleta, ele mede a concentração de metano em partes por milhão (ppm). Se a leitura passa do limite definido para aquele
ponto, um alerta é criado automaticamente e classificado por gravidade. Assim a equipe sabe não só que houve um vazamento, mas o quão sério ele é.

Esta versão é uma simulação: os sensores geram leituras de exemplo para demonstrar o funcionamento do monitoramento, dos alertas e do histórico.

## Como executar

```bash
dotnet run
```

Precisa do .NET 8 SDK.

## O que dá para fazer no menu

- Listar os sensores com status e limite de alerta
- Ver os satélites e atualizar a posição deles
- Coletar leituras agora, classificadas como NORMAL, MEDIO ou ALTO
- Ver os alertas ativos e confirmá-los
- Consultar o histórico por sensor ou por período
- Ver estatísticas de média e pico por sensor
- Simular um vazamento crítico para testar os alertas

## Organização do código

```
MetanoBR.csproj
Program.cs                 menu do console
Exceptions/                exceções de sensor e satélite
Interfaces/                IMonitorable, IAlertEmitter, IDataLogger
Models/                    Sensor, Satellite, Alert e suas subclasses
Services/                  MonitoringService e InMemoryDataLogger
Structs/                   GeoCoordinate e MethaneReading
```

O `MonitoringService` é o ponto central: registra sensores e satélites, coleta as
leituras, grava no logger e dispara alertas quando o limite de ppm é ultrapassado.
O `Program.cs` cuida só do menu e da exibição.
