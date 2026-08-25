# Black Core: Ascension — Docker VPS quick start

This stack runs MariaDB plus rAthena login/char/map servers. It is designed so the server can be deployed from a phone/Termux or Black Core terminal without a local PC.

## Start

```bash
cd deploy/docker
cp .env.example .env
# Edit .env and set PUBLIC_IP plus all passwords.
docker compose up -d --build
```

Open TCP ports **6900**, **6121**, and **5121** in the VPS firewall/security group.

## Logs

```bash
docker compose logs -f server
```

## Stop

```bash
docker compose down
```

Use `docker compose down -v` only when you intentionally want to delete the database volume and reset all accounts/characters.

## Security

- Never deploy the sample passwords from `.env.example`.
- Do not expose MariaDB port 3306 publicly; the compose file keeps it internal.
- The entrypoint replaces rAthena's `s1/p1` inter-server account at boot.
- For production, place the VPS behind appropriate firewall/DDoS controls and add an HTTPS account/auth service before accepting real user passwords from a custom mobile client.
