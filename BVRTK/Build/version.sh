#!/usr/bin/env bash

# Report unset variables and the first error in a pipe chain. 
set -uo pipefail

# Push to root of project and git repository, as this runs in the BVRTK subfolder by default.
pushd ../

# Get the latest tag, or a fallback if none exist.
latest=$(git describe --tags --abbrev=0 2>/dev/null || echo "v0.0")

# Get the second to last tag
if prev=$(git describe --tags --abbrev=0 "${latest}~1" 2>/dev/null); then
  # Extract how many commits have been made between that and the latest.
  count=$(git rev-list --count "${prev}..${latest}" 2>/dev/null || echo 0)
else
  # If there is only one tag, we check how many commits exists before that tag.
  count=$(git rev-list --count "${latest}" 2>/dev/null || echo 0)
fi

# Write the text file next to this script.
popd +0 || exit
printf '%s.%s' "$latest" "$count" > "./Build/version.txt"