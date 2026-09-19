#!/bin/sh
# Applies every pending EF Core migration, then exits.
#
# A SQL Server container accepts TCP connections a little before it will accept
# logins, and `master` is restored after that again, so the first attempts are
# expected to fail on a cold volume. Retry rather than take the whole stack down.
set -eu

: "${ConnectionStrings__DefaultConnection:?connection string is not set}"

attempts="${MIGRATE_ATTEMPTS:-30}"
delay="${MIGRATE_RETRY_DELAY:-5}"
n=1

while : ; do
    if ./efbundle --connection "$ConnectionStrings__DefaultConnection"; then
        echo "migrate: database is up to date"
        exit 0
    fi

    if [ "$n" -ge "$attempts" ]; then
        echo "migrate: giving up after $n attempts" >&2
        exit 1
    fi

    echo "migrate: attempt $n/$attempts failed, retrying in ${delay}s"
    n=$((n + 1))
    sleep "$delay"
done
