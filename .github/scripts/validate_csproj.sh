#!/bin/bash

validate_tag_value_change() {
  local removed_lines=$1
  local added_lines=$2
  local tag=$3
  
  old_version=$(echo $removed_lines | grep -oP "$tag\K[^<]+")
  new_version=$(echo $added_lines | grep -oP "$tag\K[^<]+")
  
  echo "Old $tag: $old_version"
  echo "New $tag: $new_version"
  
  if [[ -z $new_version ]]; then
    echo "$tag wasn't changed";
    exit 1;
  fi
  
  IFS='.' read -ra new_version_parts <<< $new_version
  new_major=${new_version_parts[0]}
  new_minor=${new_version_parts[1]}
  new_patch=${new_version_parts[2]}
  
  IFS='.' read -ra old_version_parts <<< $old_version
  old_major=${old_version_parts[0]}
  old_minor=${old_version_parts[1]}
  old_patch=${old_version_parts[2]}
    
  if [ $new_major -eq $old_major ] && [ $new_minor -eq $old_minor ] && [ $new_patch -le $old_patch ]; then
    echo "Patch version has been lowered or stayed the same";
    exit 1;
  elif [ $new_major -eq $old_major ] && [ $new_minor -lt $old_minor ]; then
    echo "Minor version has been lowered from $old_minor to $new_minor";
    exit 1;
  elif [ $new_major -lt $old_major ]; then
    echo "Major version has been lowered from $old_major to $new_major";
    exit 1;
  fi
}

# Use provided arguments
SOURCE_BRANCH=${SOURCE_BRANCH}
TARGET_BRANCH=${TARGET_BRANCH}
FILE_PATH=${CSPROJ_PATH}

git fetch origin $SOURCE_BRANCH:$SOURCE_BRANCH
git fetch origin $TARGET_BRANCH:$TARGET_BRANCH

# Get the changes in the target branch
CHANGES=$(git diff $TARGET_BRANCH $SOURCE_BRANCH -- $FILE_PATH)
ADDED_LINES=$(echo "$CHANGES" | grep -E '^\+' | sed 's/^\+//')
REMOVED_LINES=$(echo "$CHANGES" | grep -E '^\-' | sed 's/^\-//')

# Check Version tag
validate_tag_value_change "$REMOVED_LINES" "$ADDED_LINES" "<Version>"

# Check PackageVersion tag
validate_tag_value_change "$REMOVED_LINES" "$ADDED_LINES" "<PackageVersion>"

# Check PackageReleaseNotes tag
NEW_RELEASE_NOTES=$(echo $ADDED_LINES | grep -oP "<PackageReleaseNotes>\K[^<]+")
if [[ -z $NEW_RELEASE_NOTES ]]; then
  echo "Package release notes weren't changed";
  exit 1;
fi