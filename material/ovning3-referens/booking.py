"""Bokningssystem för padelbanor.

Referensimplementation — används av workshoptesterna för att
verifiera att testsviten fungerar med en korrekt implementation.
"""

import uuid
from dataclasses import dataclass, field
from datetime import datetime, timedelta


class BookingError(Exception):
    """Fel vid bokning eller avbokning."""


@dataclass
class Booking:
    """En bokning av en padelbana."""

    court: str
    email: str
    start: datetime
    end: datetime
    price: int
    id: str = field(default_factory=lambda: str(uuid.uuid4()))


class BookingSystem:
    """Hanterar bokningar för padelbanor."""

    def __init__(self) -> None:
        self._bookings: list[Booking] = []

    def book(self, court: str, email: str, start: datetime) -> Booking:
        """Boka en padelbana."""
        end = start + timedelta(hours=1)

        if start.hour < 7:
            raise BookingError("Bokning måste vara efter 07:00")
        if start.hour >= 22:
            raise BookingError("Bokning måste sluta före 22:00")

        for booking in self._bookings:
            if booking.court == court and booking.start == start:
                raise BookingError(f"{court} är upptagen vid denna tid")

        same_day = [
            b
            for b in self._bookings
            if b.email == email and b.start.date() == start.date()
        ]
        if len(same_day) >= 2:
            raise BookingError("Du har redan max antal bokningar denna dag")

        price = 200 if 17 <= start.hour < 20 else 100

        booking = Booking(
            court=court, email=email, start=start, end=end, price=price
        )
        self._bookings.append(booking)
        return booking

    def cancel(self, booking_id: str, current_time: datetime) -> None:
        """Avboka en bokning."""
        booking = next(
            (b for b in self._bookings if b.id == booking_id), None
        )
        if booking is None:
            raise BookingError("Bokning hittades inte")

        hours_until = (booking.start - current_time).total_seconds() / 3600
        if hours_until < 2:
            raise BookingError("Avbokning måste ske minst 2 timmar före")

        self._bookings.remove(booking)

    def list_bookings(self) -> list[Booking]:
        """Lista alla bokningar."""
        return list(self._bookings)
