"""Bokningssystem för padelbanor."""

import uuid
from dataclasses import dataclass, field
from datetime import datetime, timedelta


class BookingError(Exception):
    """Fel vid bokning eller avbokning."""


@dataclass
class Booking:
    court: str
    email: str
    start: datetime
    end: datetime
    price: int
    id: str = field(default_factory=lambda: str(uuid.uuid4()))


class BookingSystem:
    """Hanterar bokningar för padelbanor."""

    def __init__(self):
        self._bookings = []

    def book(self, court, email, start):
        end = start + timedelta(hours=1)

        # Kolla öppettider
        if start.hour < 7:
            raise BookingError("Bokning måste vara efter 07:00")
        if start.hour >= 22:
            raise BookingError("Bokning måste sluta före 22:00")

        # Kolla dubbelbokningar
        for b in self._bookings:
            if b.court == court and b.start == start:
                raise BookingError(f"{court} är upptagen vid denna tid")

        # Max 2 bokningar per person per dag
        same_day = [b for b in self._bookings
                    if b.email == email and b.start.date() == start.date()]
        if len(same_day) >= 2:
            raise BookingError("Du har redan max antal bokningar denna dag")

        # Högtrafik 17-20 kostar mer
        price = 200 if 17 <= start.hour < 20 else 100

        booking = Booking(court=court, email=email, start=start,
                         end=end, price=price)
        self._bookings.append(booking)
        return booking

    def cancel(self, booking_id, current_time):
        booking = None
        for b in self._bookings:
            if b.id == booking_id:
                booking = b
                break

        if booking is None:
            raise BookingError("Bokning hittades inte")

        hours_until = (booking.start - current_time).total_seconds() / 3600
        if hours_until < 2:
            raise BookingError("Avbokning måste ske minst 2 timmar före")

        self._bookings.remove(booking)

    def list_bookings(self):
        return list(self._bookings)
