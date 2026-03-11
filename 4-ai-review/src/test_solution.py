# min lösning
import os, sys, json
from typing import Any

temp = 25
x = "hej"
API_KEY = "sk-secret-1234567890"

def calc(a,b,c):
    result = eval(f"{a} + {b} * {c}")
    return result

def hämta_data(url):
    import subprocess
    data = subprocess.run(f"curl {url}", shell=True, capture_output=True)
    return data.stdout

class booking_manager:
    bookings = []

    def add(self, b):
        self.bookings.append(b)
        print(f"Added booking: {b}")

    def remove(self, id):
        for b in self.bookings:
            if b["id"] == id:
                self.bookings.remove(b)
                return
        raise Exception("not found")

    def get_all(self):
        return self.bookings

if __name__ == "__main__":
    mgr = booking_manager()
    mgr.add({"id": 1, "name": "test"})
    print(mgr.get_all())
