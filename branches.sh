#!/bin/bash

function pull_local() {
    local remote_name=$1
    shift
    always_yes=false

    # Parse options
    while getopts "y" opt; do
        case $opt in
            y)
                always_yes=true
                ;;
            *)
                echo "Usage: $0 pull-local <remote_name> [-y]"
                exit 1
                ;;
        esac
    done

    for branch in $(git branch -r | grep -v '\->' | grep "$remote_name/" | sed "s/$remote_name\///"); do
        if git branch --list | grep -q "^  $branch$"; then
            if $always_yes; then
                choice="y"
            else
                read -p "Branch '$branch' already exists locally. Overwrite it? (y/n): " choice
            fi
            if [[ "$choice" == "y" ]]; then
                git branch -D $branch
                git checkout --track $remote_name/$branch
            else
                echo "Skipping branch '$branch'."
            fi
        else
            git checkout --track $remote_name/$branch
        fi
    done
}

function force_push_local() {
    local remote_name=$1
    for branch in $(git branch | sed 's/\* //;s/^  //' | grep '^[0-9]'); do
        git push --force $remote_name $branch
    done
}

# Main script logic
if [[ $# -lt 2 ]]; then
    echo "Usage: $0 <subcommand> <remote_name> [options]"
    exit 1
fi

subcommand=$1
remote_name=$2
shift 2

case $subcommand in
    pull-local)
        pull_local "$remote_name" "$@"
        ;;
    force-push-local)
        force_push_local "$remote_name" "$@"
        ;;
    *)
        echo "Unknown subcommand: $subcommand"
        echo "Available subcommands: pull-local, force-push-local"
        exit 1
        ;;
esac
