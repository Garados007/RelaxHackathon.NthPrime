FROM ubuntu:latest
RUN apt-get update -y && \
    apt-get install -y --no-install-recommends wget ca-certificates && \
    wget https://packages.microsoft.com/config/ubuntu/20.10/packages-microsoft-prod.deb \
        --no-check-certificate \
        -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get install -y --no-install-recommends apt-transport-https && \
    apt-get update -y && \
    apt-get install -y --no-install-recommends dotnet-sdk-5.0 && \
    curl -sL https://deb.nodesource.com/setup_current.x | sudo -E bash - && \
    sudo apt-get install -y --no-install-recommends nodejs && \
    npm install -g redoc-cli && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /usr/src/RelaxHackathon.NthPrime
COPY . /usr/src/RelaxHackathon.NthPrime

RUN dotnet restore && \
    dotnet build -c Release && \
    chmod +x NthPrime/bin/Release/net5.0/NthPrime && \
    redoc-cli bundle swagger-api.json && \
    mv redoc-static.html NthPrime/bin/Release/net5.0/doc.html

WORKDIR /app

ENTRYPOINT [ "/usr/src/RelaxHackathon.NthPrime/NthPrime/bin/Release/net5.0/NthPrime" ]
