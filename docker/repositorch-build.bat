docker build -f "../src/Repositorch.Web/Dockerfile" --force-rm -t repositorch ".."
docker tag repositorch kirnosenko/repositorch
