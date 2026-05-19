import { Component, OnInit, Input, Output, EventEmitter, ElementRef, HostListener, ViewEncapsulation } from '@angular/core';
import * as moment from 'moment';
import { DateRangePreset, PickerSide, PickerView, NavDirection } from './enum/date-range-picker.enum';

export interface DateRangePresetItem {
  value: string;
  name: string;
}

interface CalendarDay {
  date: moment.Moment;
  day: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  isSelected: boolean;
  isInRange: boolean;
  isRangeStart: boolean;
  isRangeEnd: boolean;
}

@Component({
  selector: 'date-range-picker',
  templateUrl: './date-range-picker.component.html',
  styleUrls: ['./date-range-picker.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class DateRangePickerComponent implements OnInit {

  @Input() defaultFromDate: Date;
  @Input() defaultToDate: Date;
  @Input() presets: DateRangePresetItem[] = [
    { value: DateRangePreset.Last7Days, name: 'Last 7 days' },
    { value: DateRangePreset.Last30Days, name: 'Last 30 days' },
    { value: DateRangePreset.ThisWeek, name: 'This week' },
    { value: DateRangePreset.ThisMonth, name: 'This month' },
    { value: DateRangePreset.ThisQuarter, name: 'This quarter' },
    { value: DateRangePreset.ThisYear, name: 'This year' },
  ];

  @Output() onDateRangeChange: EventEmitter<{ fromDate: string, toDate: string }>
    = new EventEmitter<{ fromDate: string, toDate: string }>();

  dateRangeValue: Date[];
  selectedPreset: string = null;
  isOpen: boolean = false;

  leftMonth: moment.Moment;
  rightMonth: moment.Moment;
  leftCalendarDays: CalendarDay[] = [];
  rightCalendarDays: CalendarDay[] = [];
  weekDays: string[] = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su'];

  leftView: PickerView = PickerView.Days;
  rightView: PickerView = PickerView.Days;
  leftBrowseYear: number;
  rightBrowseYear: number;
  monthsList: string[] = moment.monthsShort();

  PickerSide = PickerSide;
  PickerView = PickerView;
  NavDirection = NavDirection;

  private selectionStart: moment.Moment = null;
  private selectionEnd: moment.Moment = null;
  private hoverDate: moment.Moment = null;

  constructor(private elementRef: ElementRef) {}

  ngOnInit() {
    if (this.defaultFromDate && this.defaultToDate) {
      this.dateRangeValue = [
        moment(this.defaultFromDate).toDate(),
        moment(this.defaultToDate).toDate()
      ];
    } else {
      this.dateRangeValue = [
        moment().subtract(1, 'months').startOf('month').toDate(),
        moment().endOf('month').toDate()
      ];
    }

    this.selectedPreset = null;
    this.selectionStart = moment(this.dateRangeValue[0]);
    this.selectionEnd = moment(this.dateRangeValue[1]);

    this.leftMonth = moment(this.dateRangeValue[0]).startOf('month');
    this.rightMonth = moment(this.dateRangeValue[1]).startOf('month');
    if (this.leftMonth.isSameOrAfter(this.rightMonth, 'month')) {
      this.rightMonth = this.leftMonth.clone().add(1, 'month');
    }

    this.buildCalendars();
    this.emitDateRange();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (this.isOpen && !this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen = false;
      this.leftView = PickerView.Days;
      this.rightView = PickerView.Days;
    }
  }

  togglePanel(event?: MouseEvent) {
    if (event) {
      event.stopPropagation();
    }
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.leftMonth = moment(this.dateRangeValue[0]).startOf('month');
      this.rightMonth = moment(this.dateRangeValue[1]).startOf('month');
      if (this.leftMonth.isSameOrAfter(this.rightMonth, 'month')) {
        this.rightMonth = this.leftMonth.clone().add(1, 'month');
      }
      this.selectionStart = moment(this.dateRangeValue[0]);
      this.selectionEnd = moment(this.dateRangeValue[1]);
      this.leftView = PickerView.Days;
      this.rightView = PickerView.Days;
      this.buildCalendars();
    } else {
      this.leftView = PickerView.Days;
      this.rightView = PickerView.Days;
    }
  }

  getDisplayText(): string {
    if (!this.dateRangeValue || this.dateRangeValue.length < 2) return '';
    const from = moment(this.dateRangeValue[0]).format('DD/MM/YYYY');
    const to = moment(this.dateRangeValue[1]).format('DD/MM/YYYY');
    return `${from} – ${to}`;
  }

  onPresetClick(event: MouseEvent, preset: DateRangePresetItem) {
    event.stopPropagation();
    this.selectedPreset = preset.value;
    let fromDate: moment.Moment;
    let toDate: moment.Moment = moment();

    switch (preset.value) {
      case DateRangePreset.Last7Days:
        fromDate = moment().subtract(6, 'days');
        break;
      case DateRangePreset.Last30Days:
        fromDate = moment().subtract(29, 'days');
        break;
      case DateRangePreset.ThisWeek:
        fromDate = moment().startOf('isoWeek');
        toDate = moment().endOf('isoWeek');
        break;
      case DateRangePreset.ThisMonth:
        fromDate = moment().startOf('month');
        toDate = moment().endOf('month');
        break;
      case DateRangePreset.ThisQuarter:
        fromDate = moment().startOf('quarter');
        toDate = moment().endOf('quarter');
        break;
      case DateRangePreset.ThisYear:
        fromDate = moment().startOf('year');
        toDate = moment().endOf('year');
        break;
      default:
        return;
    }

    this.dateRangeValue = [fromDate.toDate(), toDate.toDate()];
    this.selectionStart = fromDate;
    this.selectionEnd = toDate;
    this.isOpen = false;
    this.emitDateRange();
  }

  prevMonth(event: MouseEvent) {
    event.stopPropagation();
    this.leftMonth = this.leftMonth.clone().subtract(1, 'month');
    this.rightMonth = this.rightMonth.clone().subtract(1, 'month');
    this.buildCalendars();
  }

  nextMonth(event: MouseEvent) {
    event.stopPropagation();
    this.leftMonth = this.leftMonth.clone().add(1, 'month');
    this.rightMonth = this.rightMonth.clone().add(1, 'month');
    this.buildCalendars();
  }
  
  toggleView(side: PickerSide) {
    if (side === PickerSide.Left || side === PickerSide.Both) {
      this.leftView = this.leftView === PickerView.Days ? PickerView.Months : PickerView.Days;
      if (this.leftView === PickerView.Months) this.leftBrowseYear = this.leftMonth.year();
    }
    if (side === PickerSide.Right || side === PickerSide.Both) {
      this.rightView = this.rightView === PickerView.Days ? PickerView.Months : PickerView.Days;
      if (this.rightView === PickerView.Months) this.rightBrowseYear = this.rightMonth.year();
    }
  }

  navigate(event: MouseEvent, direction: NavDirection, side: PickerSide = PickerSide.Both) {
    event.stopPropagation();

    if (side === PickerSide.Left && this.leftView === PickerView.Months) {
      this.leftBrowseYear += direction === NavDirection.Prev ? -1 : 1;
      return;
    }

    if (side === PickerSide.Right && this.rightView === PickerView.Months) {
      this.rightBrowseYear += direction === NavDirection.Prev ? -1 : 1;
      return;
    }

    const amount = direction === NavDirection.Prev ? -1 : 1;
    this.leftMonth = this.leftMonth.clone().add(amount, 'month');
    this.rightMonth = this.rightMonth.clone().add(amount, 'month');
    this.buildCalendars();
  }

  selectMonth(event: MouseEvent, monthIndex: number, side: PickerSide) {
    event.stopPropagation();
    if (side === PickerSide.Left) {
      this.leftMonth = this.leftMonth.clone().year(this.leftBrowseYear).month(monthIndex);
      this.leftView = PickerView.Days;
      if (this.leftMonth.isSameOrAfter(this.rightMonth, 'month')) {
        this.rightMonth = this.leftMonth.clone().add(1, 'month');
      }
    } else {
      this.rightMonth = this.rightMonth.clone().year(this.rightBrowseYear).month(monthIndex);
      this.rightView = PickerView.Days;
      if (this.rightMonth.isSameOrBefore(this.leftMonth, 'month')) {
        this.leftMonth = this.rightMonth.clone().subtract(1, 'month');
      }
    }
    this.buildCalendars();
  }

  onDayClick(event: MouseEvent, day: CalendarDay) {
    event.stopPropagation();
    if (!day.isCurrentMonth) return;

    if (!this.selectionStart || this.selectionEnd) {
      this.selectionStart = day.date.clone();
      this.selectionEnd = null;
      this.hoverDate = null;
      this.buildCalendars();
    } else {
      if (day.date.isBefore(this.selectionStart)) {
        this.selectionEnd = this.selectionStart.clone();
        this.selectionStart = day.date.clone();
      } else {
        this.selectionEnd = day.date.clone();
      }
      this.dateRangeValue = [this.selectionStart.toDate(), this.selectionEnd.toDate()];
      this.selectedPreset = null;
      this.buildCalendars();
      this.isOpen = false;
      this.emitDateRange();
    }
  }

  onDayHover(day: CalendarDay) {
    if (this.selectionStart && !this.selectionEnd && day.isCurrentMonth) {
      if (this.hoverDate && this.hoverDate.isSame(day.date, 'day')) {
        return;
      }
      this.hoverDate = day.date.clone();
      this.buildCalendars();
    }
  }

  trackByDay(index: number, day: CalendarDay): string {
    return day.date.format('YYYY-MM-DD');
  }

  private buildCalendars() {
    this.leftCalendarDays = this.buildMonthDays(this.leftMonth);
    this.rightCalendarDays = this.buildMonthDays(this.rightMonth);
  }

  private buildMonthDays(monthStart: moment.Moment): CalendarDay[] {
    const days: CalendarDay[] = [];
    const startOfMonth = monthStart.clone().startOf('month');
    const endOfMonth = monthStart.clone().endOf('month');

    let startDay = startOfMonth.clone().isoWeekday();
    for (let i = 1; i < startDay; i++) {
      const d = startOfMonth.clone().subtract(startDay - i, 'days');
      days.push(this.createDay(d, false));
    }

    for (let d = startOfMonth.clone(); d.isSameOrBefore(endOfMonth, 'day'); d.add(1, 'day')) {
      days.push(this.createDay(d.clone(), true));
    }

    const remaining = 42 - days.length;
    const dayAfterEnd = endOfMonth.clone().add(1, 'day');
    for (let i = 0; i < remaining; i++) {
      days.push(this.createDay(dayAfterEnd.clone().add(i, 'days'), false));
    }

    return days;
  }

  private createDay(date: moment.Moment, isCurrentMonth: boolean): CalendarDay {
    const rangeStart = this.selectionStart;
    const rangeEnd = this.selectionEnd || this.hoverDate;

    let effectiveStart = rangeStart;
    let effectiveEnd = rangeEnd;
    if (effectiveStart && effectiveEnd && effectiveEnd.isBefore(effectiveStart)) {
      effectiveStart = rangeEnd;
      effectiveEnd = rangeStart;
    }

    const isRangeStart = effectiveStart ? date.isSame(effectiveStart, 'day') : false;
    const isRangeEnd = effectiveEnd ? date.isSame(effectiveEnd, 'day') : false;
    const isInRange = (effectiveStart && effectiveEnd)
      ? date.isBetween(effectiveStart, effectiveEnd, 'day', '()') 
      : false;

    return {
      date,
      day: date.date(),
      isCurrentMonth,
      isToday: date.isSame(moment(), 'day'),
      isSelected: isRangeStart || isRangeEnd,
      isInRange,
      isRangeStart,
      isRangeEnd
    };
  }

  private emitDateRange() {
    if (!this.dateRangeValue || this.dateRangeValue.length < 2) return;
    const fromDate = moment(this.dateRangeValue[0]).format('YYYY-MM-DD');
    const toDate = moment(this.dateRangeValue[1]).format('YYYY-MM-DD');
    this.onDateRangeChange.emit({ fromDate, toDate });
  }
}
