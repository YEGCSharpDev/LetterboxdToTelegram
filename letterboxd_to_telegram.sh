#!/bin/bash
# --- Required environment variables ---
# export RSS_URL="your_letterboxd_rss_url"
# export TMDB_BEARER="your_tmdb_bearer_token"
# export TELEGRAM_BOT_TOKEN="your_telegram_bot_token"
# export TELEGRAM_CHAT_ID="your_telegram_chat_id"
# export ERROR_TELEGRAM_CHAT_ID="your_error_telegram_chat_id"
# Add these to ~/.bashrc and run: source ~/.bashrc
# add cron job by going to crontab -e
# 0 */8 * * *  script_path.sh


NOW_SEC=$(date -u +%s)

clean_html() {
    echo "$1" | sed -e 's/<[^>]*>//g' | xargs
}
minutes_to_hours_minutes() {
  local mins=$1
  local h=$(( mins / 60 ))
  local m=$(( mins % 60 ))
  if [ $h -gt 0 ]; then
    echo "${h}h ${m}m"
  else
    echo "${m}m"
  fi
}
(
  set -e  # Exit if any command fails within this block

  # Fetch RSS feed
  RSS_FEED=$(curl -s "$RSS_URL")

  echo "$RSS_FEED" | awk 'BEGIN{RS="<item>"; FS="\n"} NR>1 {print $0}' | while read -r ITEM; do
      PUBDATE=$(echo "$ITEM" | grep -oPm1 "(?<=<pubDate>)[^<]+")
      PUBDATE_SEC=$(date -d "$PUBDATE" +%s 2>/dev/null)
      if [[ -z "$PUBDATE_SEC" ]]; then continue; fi
      # Check for last 8 hours (8*60*60 seconds)
      if [ $((NOW_SEC - PUBDATE_SEC)) -le 28800 ]; then
          TMDBID=$(echo "$ITEM" | grep -oPm1 "(?<=<tmdb:movieId>)[^<]+")
          TITLE=$(echo "$ITEM" | grep -oPm1 "(?<=<letterboxd:filmTitle>)[^<]+")
          RATING=$(echo "$ITEM" | grep -oPm1 "(?<=<letterboxd:memberRating>)[^<]+")
          REVIEW_RAW=$(echo "$ITEM" | grep -oP '(?<=<description><!\[CDATA\[).*?(?=\]\]>)')
          REVIEW=$(echo "$REVIEW_RAW" | sed -E 's/<img[^>]*>//g; s/<\/?p>//g' | xargs)


          # Query TMDB for movie details
          TMDB_URL="https://api.themoviedb.org/3/movie/$TMDBID?language=en-US"
          TMDB_DATA=$(curl -s \
              --url "$TMDB_URL" \
              --header "Authorization: Bearer $TMDB_BEARER" \
              --header "accept: application/json")
          LANGUAGE=$(echo "$TMDB_DATA" | grep -oP '"original_language":"\K[^"]+')
          case "$LANGUAGE" in
            ko) LANGUAGE_NAME="Korean";;
            en) LANGUAGE_NAME="English";;
            es) LANGUAGE_NAME="Spanish";;
            *) LANGUAGE_NAME="$LANGUAGE";;
          esac
          IMDB_ID=$(echo "$TMDB_DATA" | grep -oP '"imdb_id":"\K[^"]+')
          IMDB_URL="https://www.imdb.com/title/$IMDB_ID"
          GENRES=$(echo "$TMDB_DATA" | grep -oP '"genres":\[\K[^\]]+' | grep -oP '"name":"\K[^"]+' | while read -r g; do echo -n "[#$g](tg://search_hashtag?hashtag=$g) "; done)
          STORYLINE=$(echo "$TMDB_DATA" | grep -oP '"overview":"\K[^"]+')
          VOTE_AVG=$(echo "$TMDB_DATA" | grep -oP '"vote_average":\K[0-9.]+')
          VOTE_COUNT=$(echo "$TMDB_DATA" | grep -oP '"vote_count":\K[0-9]+')
          TAGLINE=$(echo "$TMDB_DATA" | grep -oP '"tagline":"\K[^"]*')
          REVENUE=$(echo "$TMDB_DATA" | grep -oP '"revenue":\K[0-9]+')
            if [[ -z "$REVENUE" || "$REVENUE" -eq 0 ]]; then
            REVENUE_LINE=""
            else
            REVENUE_FORMATTED=$(printf "%'d" "$REVENUE")
            REVENUE_LINE="Revenue: \$${REVENUE_FORMATTED}"
            fi
          RELEASE_DATE=$(echo "$TMDB_DATA" | grep -oP '"release_date":"\K[^"]+')
          RUNTIME_MINUTES=$(echo "$TMDB_DATA" | grep -oP '"runtime":\K[0-9]+')
          RUNTIME_FORMATTED=$(minutes_to_hours_minutes "$RUNTIME_MINUTES")
          # Query TMDB images API to get first poster path
          TMDB_IMAGES_URL="https://api.themoviedb.org/3/movie/$TMDBID/images"
          TMDB_IMAGES_DATA=$(curl -s \
              --url "$TMDB_IMAGES_URL" \
              --header "Authorization: Bearer $TMDB_BEARER" \
              --header "accept: application/json")
          POSTER_PATH=$(echo "$TMDB_IMAGES_DATA" | grep -oP '"posters":\[\K.*?\]' | grep -oP '"file_path":"\K[^"]+' | head -n 1)
          IMAGE_URL="https://image.tmdb.org/t/p/original$POSTER_PATH"

          TELEGRAM_MESSAGE="Title: $TITLE
Tagline: $TAGLINE
Lang: $LANGUAGE_NAME
Release Date: $RELEASE_DATE
Runtime: $RUNTIME_FORMATTED
$REVENUE_LINE
IMDB Rating: $VOTE_AVG ($VOTE_COUNT votes)
IMDB URL: [$IMDB_URL]($IMDB_URL)
Genre: $GENRES
Story Line: $STORYLINE
Jizzle's Rating and Review :
$RATING/5
$REVIEW"
          curl -s -X POST "https://api.telegram.org/bot$TELEGRAM_BOT_TOKEN/sendPhoto" \
              -F chat_id="$TELEGRAM_CHAT_ID" \
              -F photo="$IMAGE_URL" \
              -F caption="$TELEGRAM_MESSAGE" \
              -F parse_mode="Markdown"
      fi
  done

) || {
  # Catch block for any error in subshell
  ERROR_MESSAGE="Error occurred in the TMDB Telegram notification script."
  curl -s -X POST "https://api.telegram.org/bot$TELEGRAM_BOT_TOKEN/sendMessage" \
    -d chat_id="$ERROR_TELEGRAM_CHAT_ID" \
    -d text="$ERROR_MESSAGE"
}
