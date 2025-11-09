document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("eventForm");
    const eventType = document.getElementById("eventType");
    const priceGroup = document.getElementById("priceGroup");
    const priceInput = document.getElementById("price");
    const eventDateInput = document.getElementById("eventDate");

    // Prevent picking a past date
    const today = new Date().toISOString().split("T")[0];
    eventDateInput.setAttribute("min", today);

    // Toggle price field when type = paid
    function updatePriceVisibility() {
        const isPaid = eventType.value === "paid";
        priceGroup.hidden = !isPaid;
        priceInput.required = isPaid;
        if (!isPaid) {
            priceInput.value = "";
            clearInvalid(priceInput);
        }
    }
    eventType.addEventListener("change", updatePriceVisibility);
    updatePriceVisibility();

    // Validate on blur
    const requiredFields = Array.from(
        document.querySelectorAll("input[required], select[required], textarea[required]")
    );

    requiredFields.forEach((field) => {
        field.addEventListener("blur", () => validateField(field));
        field.addEventListener("input", () => {
            // live-clear when typing
            if (field.value?.length) validateField(field);
        });
    });

    // Helpers
    function setInvalid(field, message) {
        const group = field.closest(".form-group");
        if (!group) return;
        group.classList.add("invalid");
        group.classList.remove("valid");

        const msg = group.querySelector(`.validation-message[data-for="${field.id}"]`);
        if (msg) msg.textContent = message || "This field is required.";
    }

    function clearInvalid(field) {
        const group = field.closest(".form-group");
        if (!group) return;
        group.classList.remove("invalid");
        group.classList.add("valid");

        const msg = group.querySelector(`.validation-message[data-for="${field.id}"]`);
        if (msg) msg.textContent = "";
    }

    function validateField(field) {
        // Built-in validity covers required, number ranges, etc.
        if (!field.checkValidity()) {
            let message = field.validationMessage || "This field is required.";
            // Custom messages
            if (field.type === "number" && field.min && Number(field.value) < Number(field.min)) {
                message = `Must be at least ${field.min}.`;
            }
            if (field === priceInput && eventType.value === "paid" && (field.value === "" || Number(field.value) < 0)) {
                message = "Please provide a valid non-negative price for paid events.";
            }
            setInvalid(field, message);
            return false;
        } else {
            clearInvalid(field);
            return true;
        }
    }

    function validateForm() {
        let ok = true;
        // ensure price requirements reflect current selection
        updatePriceVisibility();
        requiredFields.forEach((f) => {
            if (!validateField(f)) ok = false;
        });
        return ok;
    }

    // Submit
    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        if (!validateForm()) {
            // focus first invalid field
            const firstInvalid = document.querySelector(".form-group.invalid input, .form-group.invalid select, .form-group.invalid textarea");
            if (firstInvalid) firstInvalid.focus();
            return;
        }

        const payload = buildPayload(form);
        try {
            const res = await fetch("/api/events/create", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload),
            });

            if (!res.ok) {
                const txt = await res.text();
                alert("Error creating event.\n" + txt);
                return;
            }
            // If your API returns JSON, you can read it:
            // const result = await res.json();
            window.location.href = "/EventConfirmation";
        } catch (err) {
            console.error(err);
            alert("Network error. Please try again.");
        }
    });

    function buildPayload(form) {
        const isPaid = eventType.value === "paid";
        const cap = parseInt(form.capacity.value, 10);

        return {
            EventName: form.eventName.value,
            EventDescription: form.eventDescription.value,
            StartTime: `${form.eventDate.value}T${form.eventTime.value}`,
            Address: form.eventLocation.value,
            City: form.city.value,
            Region: form.region.value,
            PostalCode: form.postalCode.value,
            Country: form.country.value,
            Capacity: cap,
            TicketType: isPaid ? 0 : 3, // adapt to your enum
            IsActive: true,
            Category: form.category.value,
            EventOrganizerId: 1, // TODO: replace with real organizer id
            CreatedAt: new Date().toISOString(),
            UpdatedAt: new Date().toISOString(),
            Prices: isPaid
                ? [{
                    Price: parseFloat(form.price.value),
                    TicketType: 0,
                    Label: "Default",
                    MaxQuantity: cap,
                    isAvailable: true
                }]
                : [{
                    Price: 0,
                    TicketType: 3,
                    Label: "Free",
                    MaxQuantity: cap,
                    isAvailable: true
                }]
        };
    }
});
