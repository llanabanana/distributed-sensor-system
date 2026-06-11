# Distributed Sensor System

Distribuirani sistem za prikupljanje, obradu i čuvanje podataka od senzorskih čvorova.  
SNUS projekat 2026 — ASP.NET Core + Docker + Kubernetes

## Arhitektura

```
SensorSimulator  →  Ingress  →  IngestionService  →  PostgreSQL
                                      ↓
                             NotificationService (SignalR)
                             ConsensusService (worker)
```

## Servisi

| Servis              | Port | Opis                                  |
| ------------------- | ---- | ------------------------------------- |
| Ingress             | 8080 | Jedina ulazna tačka, rutira saobraćaj |
| IngestionService    | 5001 | Prima podatke od senzora              |
| NotificationService | 5003 | Alarmi u realnom vremenu (SignalR)    |
| ConsensusService    | —    | Worker, računa konsenzus svakih 60s   |
| SensorSimulator     | —    | Klijent, simulira senzore             |

## Pokretanje (docker-compose)

```bash
docker-compose up --build
```

## Pokretanje (Kubernetes / Minikube)

```bash
minikube start
kubectl apply -f k8s/
```
