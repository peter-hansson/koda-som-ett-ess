"""Bokningssystem för padelbanor."""

import uuid
from datetime import datetime, timedelta


class BookingError(Exception):
    pass


class Booking:
    def __init__(self, court, email, start, end, price):
        self.court = court
        self.email = email
        self.start = start
        self.end = end
        self.price = price
        self.id = str(uuid.uuid4())


class BookingSystem:
    def __init__(self):
        self.bookings = []

    def book(self, court, email, start):
        end = start + timedelta(hours=1)

        if start.hour < 7:
            raise BookingError("Bokning måste vara efter 07:00")
        if start.hour >= 22:
            raise BookingError("Bokning måste sluta före 22:00")

        # kolla dubbelbokning
        for b in self.bookings:
            if b.court == court and b.start == start:
                raise BookingError(f"{court} är upptagen vid denna tid")

        # max 2 per dag
        count = 0
        for b in self.bookings:
            if b.email == email and b.start.date() == start.date():
                count += 1
        if count >= 2:
            raise BookingError("Du har redan max antal bokningar denna dag")

        if start.hour >= 17 and start.hour < 20:
            price = 200
        else:
            price = 100

        booking = Booking(court, email, start, end, price)
        self.bookings.append(booking)
        return booking

    def cancel(self, booking_id, current_time):
        booking = None
        for b in self.bookings:
            if b.id == booking_id:
                booking = b
                break

        if not booking:
            raise BookingError("Bokning hittades inte")

        diff = booking.start - current_time
        hours = diff.total_seconds() / 3600
        if hours < 2:
            raise BookingError("Avbokning måste ske minst 2 timmar före")

        self.bookings.remove(booking)

    def list_bookings(self):
        return list(self.bookings)
