from __future__ import annotations

import csv
import json
import re
import sys
from pathlib import Path

import requests
from bs4 import BeautifulSoup

url = "https://en.wikipedia.org/wiki/List_of_Dublin_postal_districts"
baseDir = Path(__file__).resolve().parent
jsonFile = baseDir / "dublin_postal_districts.json"
csvFile = baseDir / "dublin_postal_districts.csv"

districtHeading = re.compile(r"^Dublin\s+\d+[^()]*\((D\d+W?)\)$")

listStopMarkers = [
    ". ",
    " and contains",
    " and is ",
    " while ",
    " While ",
    " This ",
    " It ",
    " Notable",
    " Located",
]

footnotePattern = re.compile(r"\[\s*\w+\s*\]")
whitespacePattern = re.compile(r"\s+")

def get_page(url: str) -> BeautifulSoup:
    try:
        response = requests.get(
            url,
            timeout=30,
            headers={"User-Agent": "B9CY108-CA-One/1.0 (student assignment)"},
        )
        response.raise_for_status()
    except requests.RequestException as error:
        sys.exit(f"Could not fetch the Wikipedia page: {error}")

    return BeautifulSoup(response.text, "html.parser")

def clean_text(text: str) -> str:
    text = footnotePattern.sub("", text)
    return whitespacePattern.sub(" ", text).strip()

def get_text_and_links(paragraph) -> tuple[str, list[tuple[int, str]]]:
    pieces: list[str] = []
    links: list[tuple[int, str]] = []
    offset = 0

    for element in paragraph.descendants:
        if isinstance(element, str):
            piece = clean_text(str(element))
            if not piece:
                continue
            if pieces:
                offset += 1 
            pieces.append(piece)
            offset += len(piece)
        elif element.name == "a":
            name = clean_text(element.get_text(" ", strip=True))
            links.append((offset + (1 if pieces else 0), name))

    return " ".join(pieces), links

def get_areas(paragraph) -> list[str]:
    text, links = get_text_and_links(paragraph)
    lowered = text.lower()

    start = lowered.find("includes")
    if start == -1:
        return []

    areaText = text[start:]
    stop = len(areaText)
    for marker in listStopMarkers:
        found = areaText.find(marker)
        if found != -1:
            stop = min(stop, found)
    stop_index = start + stop

    for marker in ("including", "takes in"):
        position = areaText.rfind(marker, 0, stop)
        while position > 0 and areaText[position - 1] == "(":
            position = areaText.rfind(marker, 0, position)
        if position > 0:
            start += position
            break

    areas: list[str] = []
    for offset, name in links:
        if not (start <= offset < stop_index):
            continue
        if not name or re.match(r"^Dublin\b", name):
            continue
        if name not in areas:
            areas.append(name)

    return areas

def get_districts(page: BeautifulSoup) -> dict[str, list[str]]:
    districtData: dict[str, list[str]] = {}

    for heading in page.find_all("h2"):
        match = districtHeading.match(heading.get_text(" ", strip=True))
        if not match:
            continue

        code = match.group(1)
        areas: list[str] = []

        element = heading.parent.find_next_sibling()
        while element is not None:
            classes = element.get("class") or []
            if element.name == "div" and "mw-heading" in classes:
                break
            if element.name == "p":
                areas.extend(get_areas(element))
            element = element.find_next_sibling()

        unique: list[str] = []
        for area in areas:
            if area not in unique:
                unique.append(area)

        districtData[code] = unique

    return districtData

def save_json(districtData: dict[str, list[str]]) -> None:
    with jsonFile.open("w", encoding="utf-8") as file:
        json.dump(districtData, file, indent=2, ensure_ascii=False, sort_keys=True)
        file.write("\n")

def load_json() -> dict[str, list[str]]:
    with jsonFile.open(encoding="utf-8") as file:
        return json.load(file)

def sort_key(code: str) -> tuple[int, str]:
    match = re.match(r"^D(\d+)(W?)$", code)
    if not match:
        return (999, code)
    return (int(match.group(1)), match.group(2))

def save_csv(districtData: dict[str, list[str]]) -> int:
    row_count = 0
    with csvFile.open("w", encoding="utf-8", newline="") as file:
        writer = csv.writer(file)
        writer.writerow(["postal_district", "area_name"])
        for code in sorted(districtData, key=sort_key):
            for area in sorted(districtData[code]):
                writer.writerow([code, area])
                row_count += 1
    return row_count

def load_csv() -> list[tuple[str, str]]:
    with csvFile.open(encoding="utf-8", newline="") as file:
        reader = csv.DictReader(file)
        return [(row["postal_district"], row["area_name"]) for row in reader]

def show_table(csvRows: list[tuple[str, str]]) -> None:
    district_width = max([len("Postal district")] + [len(code) for code, _ in csvRows])
    area_width = max([len("Area name")] + [len(area) for _, area in csvRows])

    print()
    print(f"{'Postal district':<{district_width}}  {'Area name':<{area_width}}")
    print(f"{'-' * district_width}  {'-' * area_width}")
    for code, area in csvRows:
        print(f"{code:<{district_width}}  {area}")

def main() -> int:
    page = get_page(url)
    districtData = get_districts(page)

    if not districtData:
        sys.exit("No postal districts were parsed from the page - the page layout may have changed.")

    print(f"Districts scraped: {len(districtData)}")

    save_json(districtData)
    print(f"JSON entries written: {len(load_json())} -> {jsonFile.name}")

    row_count = save_csv(load_json())
    print(f"CSV rows written: {row_count} -> {csvFile.name}")

    csvRows = load_csv()
    print(f"CSV rows read back: {len(csvRows)}")
    show_table(csvRows)

    return 0

if __name__ == "__main__":
    sys.exit(main())
