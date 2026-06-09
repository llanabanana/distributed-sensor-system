# Bezbednosne mere

> Dokument se popunjava tokom razvoja.

## Kriptografija poruka

- **Enkripcija:** AES-256-CBC, ključ se razmenjuje pri registraciji senzora
- **Digitalni potpis:** RSA-2048 / ECDSA, svaki senzor ima sopstveni par ključeva

## Zaštita od replay napada

Svaka poruka sadrži:
- `timestamp` — vreme slanja (UTC)
- `messageId` — monotono rastući ceo broj, inkrementira se nakon svake poruke

Server odbija poruke sa `timestamp` starijim od 30 sekundi ili već viđenim `messageId`.

## Zaštita od DoS napada

- Rate limiting: ako isti ID senzora pošalje > 10 poruka/sekundi, server ga privremeno blokira
- Biblioteka: `AspNetCoreRateLimit`

## Mrežna komunikacija

- Komunikacija se odvija preko konkretne mrežne adrese (nije localhost)
- HTTP/REST između klijenata i servera
- Bezbednosni rizici i mere zaštite: _TODO_
