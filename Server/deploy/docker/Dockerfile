FROM ubuntu:24.04 AS build
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential gcc g++ make autoconf ca-certificates \
    libmariadb-dev libmariadb-dev-compat zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /opt/blackcore
COPY . .
RUN ./configure --enable-packetver=20211103 && make -j2 server

FROM ubuntu:24.04
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates mariadb-client libmariadb3 zlib1g procps \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /opt/blackcore
COPY --from=build /opt/blackcore /opt/blackcore
COPY deploy/docker/entrypoint.sh /usr/local/bin/blackcore-entrypoint
RUN chmod +x /usr/local/bin/blackcore-entrypoint /opt/blackcore/athena-start
EXPOSE 6900 6121 5121
ENTRYPOINT ["/usr/local/bin/blackcore-entrypoint"]
