# This is a sample Dockerfile with a couple of problems.
# Paste your Dockerfile here.

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
    rm -rf /var/lib/apt/lists/*

WORKDIR /usr/src/RelaxHackathon.NthPrime
COPY . /usr/src/RelaxHackathon.NthPrime

RUN dotnet restore && dotnet build && chmod +x NthPrime/bin/Debug/net5.0/NthPrime

CMD [ "/usr/src/RelaxHackathon.NthPrime/NthPrime/bin/Debug/net5.0/NthPrime" ]
