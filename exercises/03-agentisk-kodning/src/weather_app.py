import json
import urllib.request

# väderapp för svenska städer

städer = {
    "stockholm": (59.3293, 18.0686),
    "göteborg": (57.7089, 11.9746),
    "malmö": (55.6050, 13.0038),
    "uppsala": (59.8586, 17.6389),
    "linköping": (58.4108, 15.6214),
}

def hämta_väder(stad):
    if stad.lower() not in städer:
        print("Okänd stad!")
        return None
    lat, lon = städer[stad.lower()]
    url = f"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,wind_speed_10m,relative_humidity_2m&daily=temperature_2m_max,temperature_2m_min,precipitation_sum&timezone=Europe/Stockholm&forecast_days=3"
    try:
        req = urllib.request.urlopen(url)
        data = json.loads(req.read())
        return data
    except:
        print("Kunde inte hämta väder!")
        return None

def visa_väder(stad):
    data = hämta_väder(stad)
    if data is None:
        return
    current = data["current"]
    temp = current["temperature_2m"]
    vind = current["wind_speed_10m"]
    fukt = current["relative_humidity_2m"]

    print(f"\n=== Väder i {stad.title()} ===")
    print(f"Temperatur: {temp}°C")
    print(f"Vind: {vind} km/h")
    print(f"Luftfuktighet: {fukt}%")

    if temp < 0:
        print("Brrr! Det är kallt ute. Ta på dig varma kläder!")
    elif temp < 10:
        print("Lite kyligt. En jacka kan vara bra.")
    elif temp < 20:
        print("Lagom temperatur. Skön dag!")
    elif temp < 30:
        print("Varmt! Perfekt för utomhusaktiviteter.")
    else:
        print("Extremt varmt! Drick mycket vatten.")

    if vind > 20:
        print("Varning: Det blåser kraftigt!")
    elif vind > 10:
        print("Lite blåsigt idag.")

    print(f"\n--- Prognos (3 dagar) ---")
    daily = data["daily"]
    for i in range(3):
        dag = daily["time"][i]
        max_temp = daily["temperature_2m_max"][i]
        min_temp = daily["temperature_2m_min"][i]
        regn = daily["precipitation_sum"][i]
        print(f"{dag}: {min_temp}°C - {max_temp}°C", end="")
        if regn > 0:
            print(f" | Regn: {regn} mm", end="")
        print()

def jämför_städer(stad1, stad2):
    data1 = hämta_väder(stad1)
    data2 = hämta_väder(stad2)
    if data1 is None or data2 is None:
        return

    temp1 = data1["current"]["temperature_2m"]
    temp2 = data2["current"]["temperature_2m"]
    vind1 = data1["current"]["wind_speed_10m"]
    vind2 = data2["current"]["wind_speed_10m"]
    fukt1 = data1["current"]["relative_humidity_2m"]
    fukt2 = data2["current"]["relative_humidity_2m"]

    print(f"\n=== Jämförelse: {stad1.title()} vs {stad2.title()} ===")
    print(f"{'':15} {'':>12} {'':>12}")
    print(f"{'':15} {stad1.title():>12} {stad2.title():>12}")
    print(f"{'Temperatur':15} {temp1:>10}°C {temp2:>10}°C")
    print(f"{'Vind':15} {vind1:>8} km/h {vind2:>8} km/h")
    print(f"{'Luftfuktighet':15} {fukt1:>10}% {fukt2:>10}%")

    if temp1 > temp2:
        print(f"\n{stad1.title()} är {temp1-temp2:.1f}°C varmare.")
    elif temp2 > temp1:
        print(f"\n{stad2.title()} är {temp2-temp1:.1f}°C varmare.")
    else:
        print(f"\nSamma temperatur i båda städerna!")

def visa_alla():
    for stad in städer:
        visa_väder(stad)

def exportera_json(stad):
    data = hämta_väder(stad)
    if data is None:
        return
    resultat = {
        "stad": stad.title(),
        "temperatur": data["current"]["temperature_2m"],
        "vind": data["current"]["wind_speed_10m"],
        "luftfuktighet": data["current"]["relative_humidity_2m"],
        "prognos": []
    }
    daily = data["daily"]
    for i in range(3):
        resultat["prognos"].append({
            "datum": daily["time"][i],
            "max": daily["temperature_2m_max"][i],
            "min": daily["temperature_2m_min"][i],
            "regn": daily["precipitation_sum"][i]
        })
    print(json.dumps(resultat, ensure_ascii=False, indent=2))

if __name__ == "__main__":
    import sys
    if len(sys.argv) < 2:
        print("Användning: python weather_app.py <kommando> [argument]")
        print("Kommandon: visa <stad>, jämför <stad1> <stad2>, alla, json <stad>")
        sys.exit(1)

    kommando = sys.argv[1]
    if kommando == "visa" and len(sys.argv) >= 3:
        visa_väder(sys.argv[2])
    elif kommando == "jämför" and len(sys.argv) >= 4:
        jämför_städer(sys.argv[2], sys.argv[3])
    elif kommando == "alla":
        visa_alla()
    elif kommando == "json" and len(sys.argv) >= 3:
        exportera_json(sys.argv[2])
    else:
        print("Okänt kommando eller saknade argument!")
        sys.exit(1)
