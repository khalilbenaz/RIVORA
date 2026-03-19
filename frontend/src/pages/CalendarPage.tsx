import { useState, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import {
  ChevronLeft,
  ChevronRight,
  Plus,
  X,
  Clock,
  Trash2,
} from 'lucide-react';
import type { CalendarEvent } from '../api/events';

const PRESET_COLORS = ['#3b82f6', '#ef4444', '#22c55e', '#f59e0b', '#8b5cf6', '#ec4899'];

const COLOR_CLASSES: Record<string, string> = {
  '#3b82f6': 'bg-blue-500',
  '#ef4444': 'bg-red-500',
  '#22c55e': 'bg-green-500',
  '#f59e0b': 'bg-amber-500',
  '#8b5cf6': 'bg-violet-500',
  '#ec4899': 'bg-pink-500',
};

const PILL_CLASSES: Record<string, string> = {
  '#3b82f6': 'bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300',
  '#ef4444': 'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300',
  '#22c55e': 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300',
  '#f59e0b': 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',
  '#8b5cf6': 'bg-violet-100 text-violet-700 dark:bg-violet-900/40 dark:text-violet-300',
  '#ec4899': 'bg-pink-100 text-pink-700 dark:bg-pink-900/40 dark:text-pink-300',
};

function generateMockEvents(): CalendarEvent[] {
  const now = new Date();
  const events: CalendarEvent[] = [];
  const titles = [
    'Team standup', 'Sprint review', 'Design sync', 'Client call',
    'Deploy v2.1', 'Lunch meeting', 'Code review', 'Retrospective',
    'Product demo', '1:1 with manager', 'Workshop', 'Planning poker',
  ];
  for (let i = 0; i < 15; i++) {
    const day = new Date(now.getFullYear(), now.getMonth(), Math.floor(Math.random() * 28) + 1);
    const hour = 8 + Math.floor(Math.random() * 10);
    day.setHours(hour, 0, 0, 0);
    events.push({
      id: `evt-${i}`,
      title: titles[i % titles.length]!,
      description: i % 3 === 0 ? 'Some additional details about this event.' : undefined,
      startDate: day.toISOString(),
      endDate: new Date(day.getTime() + 3600000).toISOString(),
      allDay: i % 5 === 0,
      color: PRESET_COLORS[i % PRESET_COLORS.length]!,
      createdBy: 'admin',
      createdAt: new Date(Date.now() - i * 86400000).toISOString(),
    });
  }
  return events;
}

function getDaysInMonth(year: number, month: number) {
  return new Date(year, month + 1, 0).getDate();
}

function getFirstDayOfWeek(year: number, month: number) {
  const day = new Date(year, month, 1).getDay();
  return day === 0 ? 6 : day - 1; // Monday start
}

const WEEKDAYS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

export default function CalendarPage() {
  const { t } = useTranslation();
  const today = new Date();
  const [currentMonth, setCurrentMonth] = useState(today.getMonth());
  const [currentYear, setCurrentYear] = useState(today.getFullYear());
  const [events, setEvents] = useState<CalendarEvent[]>(generateMockEvents);
  const [selectedDay, setSelectedDay] = useState<number | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [isMobileList, setIsMobileList] = useState(false);

  // Form state
  const [formTitle, setFormTitle] = useState('');
  const [formDate, setFormDate] = useState('');
  const [formTime, setFormTime] = useState('09:00');
  const [formColor, setFormColor] = useState<string>(PRESET_COLORS[0]!);
  const [formDescription, setFormDescription] = useState('');
  const [formAllDay, setFormAllDay] = useState(false);

  const daysInMonth = getDaysInMonth(currentYear, currentMonth);
  const firstDay = getFirstDayOfWeek(currentYear, currentMonth);

  const prevMonthDays = getDaysInMonth(
    currentMonth === 0 ? currentYear - 1 : currentYear,
    currentMonth === 0 ? 11 : currentMonth - 1
  );

  const eventsByDay = useMemo(() => {
    const map: Record<number, CalendarEvent[]> = {};
    events.forEach((e) => {
      const d = new Date(e.startDate);
      if (d.getMonth() === currentMonth && d.getFullYear() === currentYear) {
        const day = d.getDate();
        if (!map[day]) map[day] = [];
        map[day].push(e);
      }
    });
    return map;
  }, [events, currentMonth, currentYear]);

  const goToday = () => {
    setCurrentMonth(today.getMonth());
    setCurrentYear(today.getFullYear());
  };

  const prevMonth = () => {
    if (currentMonth === 0) {
      setCurrentMonth(11);
      setCurrentYear(currentYear - 1);
    } else {
      setCurrentMonth(currentMonth - 1);
    }
    setSelectedDay(null);
  };

  const nextMonth = () => {
    if (currentMonth === 11) {
      setCurrentMonth(0);
      setCurrentYear(currentYear + 1);
    } else {
      setCurrentMonth(currentMonth + 1);
    }
    setSelectedDay(null);
  };

  const monthName = new Date(currentYear, currentMonth).toLocaleString('default', { month: 'long', year: 'numeric' });

  const isToday = (day: number) =>
    day === today.getDate() && currentMonth === today.getMonth() && currentYear === today.getFullYear();

  const handleAddEvent = () => {
    if (!formTitle.trim() || !formDate) return;
    const startDate = formAllDay
      ? new Date(`${formDate}T00:00:00`).toISOString()
      : new Date(`${formDate}T${formTime}:00`).toISOString();
    const newEvent: CalendarEvent = {
      id: `evt-${Date.now()}`,
      title: formTitle,
      description: formDescription || undefined,
      startDate,
      allDay: formAllDay,
      color: formColor,
      createdBy: 'admin',
      createdAt: new Date().toISOString(),
    };
    setEvents((prev) => [...prev, newEvent]);
    setFormTitle('');
    setFormDate('');
    setFormTime('09:00');
    setFormDescription('');
    setFormAllDay(false);
    setShowForm(false);
  };

  const handleDeleteEvent = (id: string) => {
    setEvents((prev) => prev.filter((e) => e.id !== id));
  };

  const selectedDayEvents = selectedDay ? eventsByDay[selectedDay] ?? [] : [];

  // Build calendar grid cells
  const cells: { day: number; inMonth: boolean }[] = [];
  for (let i = 0; i < firstDay; i++) {
    cells.push({ day: prevMonthDays - firstDay + 1 + i, inMonth: false });
  }
  for (let d = 1; d <= daysInMonth; d++) {
    cells.push({ day: d, inMonth: true });
  }
  const remaining = 42 - cells.length;
  for (let i = 1; i <= remaining; i++) {
    cells.push({ day: i, inMonth: false });
  }

  // Mobile: get events for selected week
  const mobileWeekEvents = useMemo(() => {
    if (!isMobileList) return [];
    const startOfWeek = selectedDay ?? today.getDate();
    const dayOfWeek = new Date(currentYear, currentMonth, startOfWeek).getDay();
    const mondayOffset = dayOfWeek === 0 ? -6 : 1 - dayOfWeek;
    const weekStart = startOfWeek + mondayOffset;
    const weekDays: CalendarEvent[] = [];
    for (let d = weekStart; d < weekStart + 7; d++) {
      if (eventsByDay[d]) weekDays.push(...eventsByDay[d]!);
    }
    return weekDays.sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime());
  }, [isMobileList, selectedDay, eventsByDay, currentMonth, currentYear, today]);

  return (
    <div>
      {/* Header */}
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{t('calendar.title')}</h1>
        <div className="flex items-center gap-2">
          <button
            onClick={() => setIsMobileList(!isMobileList)}
            className="rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-600 transition hover:bg-slate-100 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700 md:hidden"
          >
            {isMobileList ? t('calendar.gridView') : t('calendar.listView')}
          </button>
          <button
            onClick={() => { setShowForm(true); setFormDate(`${currentYear}-${String(currentMonth + 1).padStart(2, '0')}-${String(selectedDay ?? today.getDate()).padStart(2, '0')}`); }}
            className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
          >
            <Plus size={16} /> {t('calendar.addEvent')}
          </button>
        </div>
      </div>

      {/* Month navigation */}
      <div className="mb-4 flex items-center gap-3">
        <button onClick={prevMonth} className="rounded-lg p-2 text-slate-600 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-700">
          <ChevronLeft size={20} />
        </button>
        <h2 className="text-lg font-semibold capitalize text-slate-800 dark:text-slate-100">{monthName}</h2>
        <button onClick={nextMonth} className="rounded-lg p-2 text-slate-600 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-700">
          <ChevronRight size={20} />
        </button>
        <button
          onClick={goToday}
          className="ml-2 rounded-lg border border-slate-300 px-3 py-1.5 text-xs font-medium text-slate-600 transition hover:bg-slate-100 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700"
        >
          {t('calendar.today')}
        </button>
      </div>

      {/* Add Event Form */}
      {showForm && (
        <div className="mb-6 rounded-xl border border-slate-200 bg-white p-5 shadow-sm dark:border-slate-700 dark:bg-slate-800">
          <div className="mb-4 flex items-center justify-between">
            <h3 className="font-semibold text-slate-800 dark:text-white">{t('calendar.newEvent')}</h3>
            <button onClick={() => setShowForm(false)} className="text-slate-400 hover:text-slate-600"><X size={18} /></button>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <input
              type="text"
              placeholder={t('calendar.eventTitle')}
              value={formTitle}
              onChange={(e) => setFormTitle(e.target.value)}
              className="rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-slate-600 dark:bg-slate-700 dark:text-white"
            />
            <input
              type="date"
              value={formDate}
              onChange={(e) => setFormDate(e.target.value)}
              className="rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-slate-600 dark:bg-slate-700 dark:text-white"
            />
            {!formAllDay && (
              <input
                type="time"
                value={formTime}
                onChange={(e) => setFormTime(e.target.value)}
                className="rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 dark:border-slate-600 dark:bg-slate-700 dark:text-white"
              />
            )}
            <label className="flex items-center gap-2 text-sm text-slate-600 dark:text-slate-300">
              <input type="checkbox" checked={formAllDay} onChange={(e) => setFormAllDay(e.target.checked)} className="rounded" />
              {t('calendar.allDay')}
            </label>
            <div className="flex items-center gap-2">
              {PRESET_COLORS.map((c) => (
                <button
                  key={c}
                  onClick={() => setFormColor(c)}
                  className={`h-7 w-7 rounded-full ${COLOR_CLASSES[c]} transition ${formColor === c ? 'ring-2 ring-offset-2 ring-slate-400' : 'opacity-60 hover:opacity-100'}`}
                />
              ))}
            </div>
            <textarea
              placeholder={t('calendar.description')}
              value={formDescription}
              onChange={(e) => setFormDescription(e.target.value)}
              rows={2}
              className="rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 sm:col-span-2 dark:border-slate-600 dark:bg-slate-700 dark:text-white"
            />
          </div>
          <div className="mt-4 flex gap-2">
            <button
              onClick={handleAddEvent}
              disabled={!formTitle.trim() || !formDate}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:opacity-50"
            >
              {t('common.save')}
            </button>
            <button onClick={() => setShowForm(false)} className="rounded-lg border border-slate-300 px-4 py-2 text-sm text-slate-600 transition hover:bg-slate-100 dark:border-slate-600 dark:text-slate-300">
              {t('common.cancel')}
            </button>
          </div>
        </div>
      )}

      <div className="flex flex-col gap-6 lg:flex-row">
        {/* Calendar Grid (hidden on mobile when list view is active) */}
        <div className={`flex-1 ${isMobileList ? 'hidden md:block' : ''}`}>
          <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800">
            {/* Weekday headers */}
            <div className="grid grid-cols-7 border-b border-slate-200 dark:border-slate-700">
              {WEEKDAYS.map((d) => (
                <div key={d} className="px-2 py-2 text-center text-xs font-semibold uppercase tracking-wider text-slate-500 dark:text-slate-400">
                  {d}
                </div>
              ))}
            </div>
            {/* Day cells */}
            <div className="grid grid-cols-7">
              {cells.map((cell, idx) => {
                const dayEvents = cell.inMonth ? eventsByDay[cell.day] ?? [] : [];
                const selected = cell.inMonth && selectedDay === cell.day;
                return (
                  <button
                    key={idx}
                    onClick={() => cell.inMonth && setSelectedDay(cell.day)}
                    className={`relative min-h-[80px] border-b border-r border-slate-100 p-1.5 text-left transition hover:bg-slate-50 dark:border-slate-700 dark:hover:bg-slate-700/50
                      ${!cell.inMonth ? 'text-slate-300 dark:text-slate-600' : 'text-slate-700 dark:text-slate-200'}
                      ${selected ? 'bg-blue-50 dark:bg-blue-900/20' : ''}
                    `}
                  >
                    <span
                      className={`inline-flex h-7 w-7 items-center justify-center rounded-full text-sm font-medium
                        ${isToday(cell.day) && cell.inMonth ? 'ring-2 ring-blue-500 bg-blue-500 text-white' : ''}
                      `}
                    >
                      {cell.day}
                    </span>
                    <div className="mt-0.5 flex flex-wrap gap-0.5">
                      {dayEvents.slice(0, 3).map((e) => (
                        <span
                          key={e.id}
                          className={`block truncate rounded px-1 py-0.5 text-[10px] font-medium leading-tight ${PILL_CLASSES[e.color] ?? 'bg-slate-100 text-slate-600'}`}
                          title={e.title}
                        >
                          {e.title}
                        </span>
                      ))}
                      {dayEvents.length > 3 && (
                        <span className="text-[10px] text-slate-400">+{dayEvents.length - 3}</span>
                      )}
                    </div>
                  </button>
                );
              })}
            </div>
          </div>
        </div>

        {/* Mobile list view */}
        {isMobileList && (
          <div className="md:hidden">
            <h3 className="mb-3 text-sm font-semibold text-slate-600 dark:text-slate-300">{t('calendar.weekEvents')}</h3>
            {mobileWeekEvents.length === 0 ? (
              <p className="text-sm text-slate-400">{t('calendar.noEvents')}</p>
            ) : (
              <div className="space-y-2">
                {mobileWeekEvents.map((e) => (
                  <div key={e.id} className={`rounded-lg p-3 ${PILL_CLASSES[e.color] ?? 'bg-slate-100'}`}>
                    <div className="font-medium">{e.title}</div>
                    <div className="mt-1 flex items-center gap-1 text-xs opacity-75">
                      <Clock size={12} />
                      {e.allDay ? t('calendar.allDay') : new Date(e.startDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                      {' - '}
                      {new Date(e.startDate).toLocaleDateString([], { weekday: 'short', day: 'numeric' })}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Side panel: selected day events */}
        {selectedDay !== null && (
          <div className="w-full shrink-0 lg:w-80">
            <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-800">
              <div className="mb-3 flex items-center justify-between">
                <h3 className="font-semibold text-slate-800 dark:text-white">
                  {new Date(currentYear, currentMonth, selectedDay).toLocaleDateString('default', { weekday: 'long', day: 'numeric', month: 'long' })}
                </h3>
                <button onClick={() => setSelectedDay(null)} className="text-slate-400 hover:text-slate-600"><X size={16} /></button>
              </div>
              {selectedDayEvents.length === 0 ? (
                <p className="text-sm text-slate-400">{t('calendar.noEvents')}</p>
              ) : (
                <div className="space-y-2">
                  {selectedDayEvents.map((e) => (
                    <div key={e.id} className={`rounded-lg p-3 ${PILL_CLASSES[e.color] ?? 'bg-slate-100'}`}>
                      <div className="flex items-start justify-between">
                        <div className="font-medium">{e.title}</div>
                        <button onClick={() => handleDeleteEvent(e.id)} className="ml-2 opacity-50 hover:opacity-100"><Trash2 size={14} /></button>
                      </div>
                      {e.description && <p className="mt-1 text-xs opacity-75">{e.description}</p>}
                      <div className="mt-1 flex items-center gap-1 text-xs opacity-75">
                        <Clock size={12} />
                        {e.allDay ? t('calendar.allDay') : new Date(e.startDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
