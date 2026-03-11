"""Tkinter-gränssnitt för bokningssystemet.

Referensimplementation — används av workshoptesterna för att
verifiera att UI-testsviten fungerar med en korrekt implementation.
"""

import tkinter as tk
from datetime import datetime

from src.booking import BookingError, BookingSystem


class BookingApp:
    """Grafiskt gränssnitt för padelbanebokning."""

    def __init__(self, root: tk.Tk) -> None:
        self.root = root
        self.root.title("Padelbane-bokning")
        self.system = BookingSystem()
        self._booking_ids: list[str] = []

        # Inmatningsfält
        tk.Label(root, text="Bana:").grid(row=0, column=0, sticky="e")
        self.court_entry = tk.Entry(root)
        self.court_entry.grid(row=0, column=1)

        tk.Label(root, text="E-post:").grid(row=1, column=0, sticky="e")
        self.email_entry = tk.Entry(root)
        self.email_entry.grid(row=1, column=1)

        tk.Label(root, text="Tid (YYYY-MM-DD HH:MM):").grid(
            row=2, column=0, sticky="e"
        )
        self.time_entry = tk.Entry(root)
        self.time_entry.grid(row=2, column=1)

        # Knappar
        self.book_button = tk.Button(root, text="Boka", command=self._book)
        self.book_button.grid(row=3, column=0)

        self.cancel_button = tk.Button(
            root, text="Avboka", command=self._cancel
        )
        self.cancel_button.grid(row=3, column=1)

        # Status
        self._status_var = tk.StringVar()
        tk.Label(root, textvariable=self._status_var).grid(
            row=4, column=0, columnspan=2
        )

        # Bokningslista
        self._listbox = tk.Listbox(root, width=60)
        self._listbox.grid(row=5, column=0, columnspan=2)

    def _book(self) -> None:
        court = self.court_entry.get()
        email = self.email_entry.get()
        time_str = self.time_entry.get()

        try:
            start = datetime.strptime(time_str, "%Y-%m-%d %H:%M")
        except ValueError:
            self._status_var.set("Ogiltigt tidsformat")
            return

        try:
            booking = self.system.book(court, email, start)
        except BookingError as e:
            self._status_var.set(str(e))
            return

        self._booking_ids.append(booking.id)
        display = (
            f"{booking.court} | {booking.email} | "
            f"{booking.start.strftime('%Y-%m-%d %H:%M')} | {booking.price} kr"
        )
        self._listbox.insert(tk.END, display)
        self._status_var.set(
            f"Bokad: {court} kl {start.strftime('%H:%M')}"
        )

        self.court_entry.delete(0, tk.END)
        self.email_entry.delete(0, tk.END)
        self.time_entry.delete(0, tk.END)

    def _cancel(self) -> None:
        selection = self._listbox.curselection()
        if not selection:
            return
        index = selection[0]
        booking_id = self._booking_ids[index]

        try:
            self.system.cancel(booking_id, current_time=datetime.now())
        except BookingError as e:
            self._status_var.set(str(e))
            return

        self._listbox.delete(index)
        self._booking_ids.pop(index)
        self._status_var.set("Bokning avbokad")

    def get_bookings_display(self) -> list[str]:
        """Returnera alla bokningar som visas i listan."""
        return [self._listbox.get(i) for i in range(self._listbox.size())]

    def get_status_message(self) -> str:
        """Returnera aktuellt statusmeddelande."""
        return self._status_var.get()

    def select_booking(self, index: int) -> None:
        """Markera en bokning i listan."""
        self._listbox.selection_set(index)
