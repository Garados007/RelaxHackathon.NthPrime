# RelaxHackathon.NthPrime

> This project was part of a Hackathon and will no longer be maintained. If you think you can extend it feel free to create a fork.

# hackathon-example-submission

This project was created in the Relaxdays Code Challenge Vol. 1. See
https://sites.google.com/relaxdays.de/hackathon-relaxdays/startseite for more information. My
participant ID in the challenge was: CC-VOL1-49

## How to run this project
You can get a running version of this code by using:
```bash
git clone https://github.com/Garados007/RelaxHackathon.NthPrime.git
cd hackathon-example-submission
docker build -t hackathon-example .
docker run -v $(pwd):/app -p 8080:8001 -it hackathon-example
```
If you now access http://localhost:8080/ you should see the thing you want to review.

For example:
```bash
$ curl http://localhost:8080/n-th-prime -d "n=200"
1223
```

To use a local file you need to call the docker container with:
```bash
docker run -v $(pwd):/app -it hackathon-example testinstance.json
```

## Api

The swagger api is accessible here:
- [SwaggerHub](https://app.swaggerhub.com/apis/Garados007/Relaxday-Hackathon-Example-49/1.0.0)
- [git](swagger-api.yaml)
