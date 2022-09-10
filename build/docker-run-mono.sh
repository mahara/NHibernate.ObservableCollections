#!/bin/bash

set -e
MONO_TAG=${MONO_TAG:-6.12.0.182}
docker run --rm -v "$PWD":'/project' -w='/project' mono:$MONO_TAG mono "$@"
