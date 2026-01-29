// Core layout interactions and per-page behavior

document.addEventListener("DOMContentLoaded", () => {
  initializeSidebarToggle();
  initializeAuthPasswordToggles();
  initializeInboxRowClicks();
  initializeCharts();
  initializeSummernoteEditor();
});

// Sidebar toggle for mobile
function initializeSidebarToggle() {
  const sidebar = document.querySelector(".layout__sidebar");
  const toggle = document.querySelector("[data-sidebar-toggle]");

  if (!sidebar || !toggle) return;

  toggle.addEventListener("click", () => {
    sidebar.classList.toggle("layout__sidebar--open");
  });
}

// Password visibility toggles on auth pages
function initializeAuthPasswordToggles() {
  const toggles = document.querySelectorAll("[data-password-toggle]");

  toggles.forEach((toggle) => {
    const targetSelector = toggle.getAttribute("data-password-toggle");
    const input = document.querySelector(targetSelector);

    if (!input) return;

    toggle.addEventListener("click", () => {
      const isPassword = input.getAttribute("type") === "password";
      input.setAttribute("type", isPassword ? "text" : "password");
      toggle.classList.toggle("fa-eye");
      toggle.classList.toggle("fa-eye-slash");
    });
  });
}

// Make inbox table rows navigable to email detail
function initializeInboxRowClicks() {
  const rows = document.querySelectorAll("[data-email-link]");

  rows.forEach((row) => {
    row.addEventListener("click", (event) => {
      // Avoid triggering when clicking actual inputs or buttons
      const target = event.target;
      if (
        target instanceof HTMLElement &&
        (target.closest("input") || target.closest("button") || target.closest("a"))
      ) {
        return;
      }

      window.location.href = "email-detail.html";
    });
  });
}

// Initialize charts on dashboard
function initializeCharts() {
  if (typeof Chart === "undefined") {
    return;
  }

  const sessionEl = document.getElementById("chart-sessions");
  const visitorsEl = document.getElementById("chart-visitors");
  const pageviewsEl = document.getElementById("chart-pageviews");
  const newReturningEl = document.getElementById("chart-new-vs-returning");
  const devicesEl = document.getElementById("chart-devices");

  if (sessionEl) {
    new Chart(sessionEl, {
      type: "line",
      data: {
        labels: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
        datasets: [
          {
            label: "Sessions",
            data: [320, 410, 365, 460, 520, 480, 530],
            borderColor: "#7C3AED",
            backgroundColor: "rgba(124, 58, 237, 0.2)",
            tension: 0.35,
            fill: true,
          },
        ],
      },
      options: baseLineChartOptions(),
    });
  }

  if (visitorsEl) {
    new Chart(visitorsEl, {
      type: "line",
      data: {
        labels: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
        datasets: [
          {
            label: "Visitors",
            data: [210, 240, 230, 260, 300, 280, 315],
            borderColor: "#3B82F6",
            backgroundColor: "rgba(59, 130, 246, 0.2)",
            tension: 0.35,
            fill: true,
          },
        ],
      },
      options: baseLineChartOptions(),
    });
  }

  if (pageviewsEl) {
    new Chart(pageviewsEl, {
      type: "line",
      data: {
        labels: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
        datasets: [
          {
            label: "Page Views",
            data: [820, 910, 865, 1040, 1120, 980, 1250],
            borderColor: "#EC4899",
            backgroundColor: "rgba(236, 72, 153, 0.2)",
            tension: 0.35,
            fill: true,
          },
        ],
      },
      options: baseLineChartOptions(),
    });
  }

  if (newReturningEl) {
    new Chart(newReturningEl, {
      type: "bar",
      data: {
        labels: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
        datasets: [
          {
            label: "New",
            data: [60, 70, 65, 80, 90, 75, 95],
            backgroundColor: "#7C3AED",
            borderRadius: 8,
            barThickness: 14,
          },
          {
            label: "Returning",
            data: [40, 45, 42, 50, 55, 52, 60],
            backgroundColor: "#3B82F6",
            borderRadius: 8,
            barThickness: 14,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: true,
            labels: {
              color: "#9CA3AF",
              usePointStyle: true,
            },
          },
          tooltip: {
            backgroundColor: "#020617",
            borderColor: "#1F2937",
            borderWidth: 1,
          },
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: { color: "#6B7280" },
          },
          y: {
            grid: { color: "rgba(55, 65, 81, 0.5)" },
            ticks: { color: "#6B7280" },
          },
        },
      },
    });
  }

  if (devicesEl) {
    new Chart(devicesEl, {
      type: "doughnut",
      data: {
        labels: ["Desktop", "Mobile", "Tablet"],
        datasets: [
          {
            data: [58, 32, 10],
            backgroundColor: ["#7C3AED", "#EC4899", "#3B82F6"],
            borderWidth: 0,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: "bottom",
            labels: {
              color: "#9CA3AF",
              usePointStyle: true,
            },
          },
        },
        cutout: "70%",
      },
    });
  }
}

function baseLineChartOptions() {
  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: "#020617",
        borderColor: "#1F2937",
        borderWidth: 1,
      },
    },
    scales: {
      x: {
        grid: { display: false },
        ticks: { color: "#6B7280" },
      },
      y: {
        grid: { color: "rgba(55, 65, 81, 0.5)" },
        ticks: { color: "#6B7280" },
      },
    },
  };
}

// Initialize Summernote editor on compose page
function initializeSummernoteEditor() {
  const editor = document.getElementById("compose-editor");
  if (!editor || typeof $ === "undefined" || typeof $.fn.summernote === "undefined") {
    return;
  }

  $("#compose-editor").summernote({
    placeholder: "Write your email content...",
    height: 260,
    minHeight: 180,
    toolbar: [
      ["style", ["bold", "italic", "underline", "clear"]],
      ["font", ["fontsize"]],
      ["para", ["ul", "ol", "paragraph"]],
      ["insert", ["link", "picture"]],
      ["view", ["fullscreen", "codeview"]],
    ],
  });
}

