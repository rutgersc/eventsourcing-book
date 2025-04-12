#!/bin/bash

REMOTE_NAME="rutgersc"

function pull_local() {
    always_yes=false

    # Parse options
    while getopts "y" opt; do
        case $opt in
            y)
                always_yes=true
                ;;
            *)
                echo "Usage: $0 pull-local [-y]"
                exit 1
                ;;
        esac
    done

    for branch in $(git branch -r | grep -v '\->' | grep "$REMOTE_NAME/" | sed "s/$REMOTE_NAME\///"); do
        if git branch --list | grep -q "^  $branch$"; then
            if $always_yes; then
                choice="y"
            else
                read -p "Branch '$branch' already exists locally. Overwrite it? (y/n): " choice
            fi
            if [[ "$choice" == "y" ]]; then
                git branch -D $branch
                git checkout --track $REMOTE_NAME/$branch
            else
                echo "Skipping branch '$branch'."
            fi
        else
            git checkout --track $REMOTE_NAME/$branch
        fi
    done
}

function force_push_local() {
    for branch in $(git branch | sed 's/\* //;s/^  //' | grep '^[0-9]'); do
        git push --force $REMOTE_NAME $branch
    done
}

# Main script logic
if [[ $# -lt 1 ]]; then
    echo "Usage: $0 <subcommand> [options]"
    exit 1
fi

subcommand=$1
shift

case $subcommand in
    pull-local)
        pull_local "$@"
        ;;
    force-push-local)
        force_push_local "$@"
        ;;
    *)
        echo "Unknown subcommand: $subcommand"
        echo "Available subcommands: pull-local, force-push-local"
        exit 1
        ;;
esac
