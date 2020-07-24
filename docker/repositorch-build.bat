docker build -f "../src/Repositorch.Web/Dockerfile" --force-rm -t repositorch "../src"
docker tag repositorch kirnosenko/repositorch
