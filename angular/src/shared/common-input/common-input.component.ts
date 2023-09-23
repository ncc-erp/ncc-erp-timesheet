import { CommonInputService } from './common-input.service';
import {
  AfterViewInit,
  Component,
  ComponentFactoryResolver,
  ElementRef,
  forwardRef,
  HostListener,
  Injector,
  Input,
  OnInit,
  Renderer2,
  ViewChild,
  ViewContainerRef,
  Output,
  EventEmitter,
  Optional,
  Host,
  SkipSelf,
  OnDestroy,
  AfterViewChecked,
} from '@angular/core';
import { AbstractControl, ControlValueAccessor, NG_VALUE_ACCESSOR, NgControl, FormGroup, ControlContainer } from '@angular/forms';
import { MESSAGE_VALIDATION_RULE } from './message.validattion';
import { InputTypeBase } from './input-type.interface';

@Component({
  selector: 'common-input',
  templateUrl: './common-input.component.html',
  styleUrls: ['./common-input.component.less'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CommonInputComponent),
      multi: true
    }
  ]
})
export class CommonInputComponent implements OnInit, OnDestroy, AfterViewInit, ControlValueAccessor {
// only have one type
@ViewChild('inputElement', { read: ViewContainerRef }) set entry(et: ViewContainerRef) {
  if (et && !this.componentRef) {
    this._entry = et;
    this.createInputComponent(et);
  }
}

_entry: ViewContainerRef;
componentRef: any;

// @Input() defaultClass: boolean = true;
@Input() md: string; // 3-7 grid-md
@Input() xl: string; // 3-7 grid-md
@Input() lg: string; // 3-7 grid-md
@Input() sm: string; // 3-7 grid-md
@Input() placeholder = '';
@Input() label: string;
@Input() inline = true;
@Input() fullWidth = false;
@Input() formControl: AbstractControl;
@Input() errorMessages: any;
@Input() type: any = 'text';
@Input() readOnly: Boolean = false;
@Input() labelAlign: String = 'text-lg-right';
@Input() step: number;

_disabled: Boolean = false;
@Input()
set disabled(disabled: boolean) {
  this._disabled = disabled;

  if (!this.formControl) {
    return;
  }
  this.disable();
}

@Output() selected: EventEmitter<any> = new EventEmitter<any>();
@Output() removed: EventEmitter<any> = new EventEmitter<any>();

_display: Boolean = true;
get display() {
  return this._display;
}

@Input()
set display(show: Boolean) {
  // console.log(show);
  this._display = show;
  
  this.setDisplay();
}

_data: any = {};
@Input()
get data() {
  return this._data;
}
set data(data: any) {
  this._data = data;
  this.initDataForChild(data);
}

@Input()
labelClass = '';

@Input()
inputClass = '';

@Input()
formControlName: string;

errorMsg = '';

_onChange: any;
_onTouched: any;
constructor(
  private _elementRef: ElementRef,
  private _renderer: Renderer2,
  private injector: Injector,
  private resolver: ComponentFactoryResolver,
  private helper: CommonInputService,
  @Optional() @Host() @SkipSelf()
  private controlContainer: ControlContainer,
) { }

writeValue(value: any): void {
  this._renderer.setProperty(this._elementRef.nativeElement, 'value', value);
}

@HostListener('change') change($event) {
  if ($event && $event.target) {
    this._onChange($event.target.value);
  }
}

registerOnChange(fn: any): void {
  this._onChange = fn;
}

@HostListener('blur') blur() {
  this._onTouched();
}

registerOnTouched(fn: any): void {
  this._onTouched = fn;
}

setDisabledState?(isDisabled: boolean): void {
  this._renderer.setProperty(this._elementRef.nativeElement, 'disabled', isDisabled);
}

ngOnInit(): void {
  if (!this.md && !this.xl && !this.lg && !this.sm) {
    this.md = this.xl = '3-7';
  }

  // console.log(this.formControl);
  this.initClassForLabelAndInput();
  if (this.controlContainer && this.formControlName) {
    this.formControl = this.controlContainer.control.get(this.formControlName);
  }
  // set some default
  if (this.formControl && this.formControl.enabled) {
    this.setDisplay();    
  } else {
    this._disabled = true;
  }
  
  if (this._display) {
    this.disable();
  }
}

setDisplay(): any {
  if (!this.formControl) {
    return;
  }
  
  if (this._display && this.formControl.disabled) {
    console.log(this.getNameControl(), 'enable');
    this.formControl.enable();
  }

  if (!this._display && this.formControl.enabled) {
    // use ngif
    this.componentRef = null;
    console.log(this.getNameControl(), 'disable');
    this.formControl.disable();
  }
}

ngAfterViewInit(): void {
}

private disable() {
  if (this._disabled) {
    this.formControl.disable({
      emitEvent: false
    });
  } else {
    this.formControl.enable({
      emitEvent: false
    });
  }
}

createInputComponent(entry: ViewContainerRef) {
  console.log(entry);
  entry.clear();
  const component = this.helper.getInputComponentByType(this.type);

  if (!component) {
    alert('error');
  }
  const factory = this.resolver.resolveComponentFactory(component);
  this.componentRef = entry.createComponent(factory);
  console.log(this.componentRef);

  (<InputTypeBase<any>>this.componentRef.instance).formControlInput = this.formControl;
  this.initDataForChild(this.data);
}

initDataForChild(data: any) {
  if (this.componentRef && data) {
    data.placeholder = this.placeholder;

    data.selected = this.selected;
    data.removed = this.removed;
    (<InputTypeBase<any>>this.componentRef.instance).data = data;
  }
}

initClassForLabelAndInput() {
  // extend for sm,...
  if (this.sm) {
    const smArray = this.sm.split('-');
    this.labelClass += ' col-sm-' + smArray[0];
    this.inputClass += ' col-sm-' + smArray[1];
  }

  if (this.md) {
    const mdArray = this.md.split('-');
    this.labelClass += ' col-md-' + mdArray[0];
    this.inputClass += ' col-md-' + mdArray[1];
  }

  if (this.xl) {
    const xlArray = this.xl.split('-');
    this.labelClass += ' col-xl-' + xlArray[0];
    this.inputClass += ' col-xl-' + xlArray[1];
  }

  if (this.lg) {
    const lgArray = this.lg.split('-');
    this.labelClass += ' col-lg-' + lgArray[0];
    this.inputClass += ' col-lg-' + lgArray[1];
  }
}

checkRequired() {
  if (!this.formControl) return false;
  const abstractControl = this.formControl;
  if (abstractControl.validator) {
    const validator = abstractControl.validator({} as AbstractControl);
    if (validator && validator.required) {
      return true;
    }
  }

  return false;
}

checkError() {
  if (!this.formControl) return false;

  const control = this.formControl;
  if (control.errors && (control.touched || control.dirty)) {
    // set active error
    this.errorMsg = this.getErrorMsg(control.errors);

    return true;
  }

  this.errorMsg = '';
  return false;
}

getErrorMsg(errors) {
  if (!this.formControl) return '';

  let rule;
  let msg = '';
  let nameControl = this.getNameControl();

  for (const key in errors) {
    if (errors[key]) {
      rule = key;
      break;
    }
  }
  // console.log(nameControl);

  if (this.errorMessages && this.errorMessages[rule]) {
    return this.errorMessages[rule];
  }

  if (this.errorMessages && this.errorMessages[nameControl] && this.errorMessages[nameControl][rule]) {
    return this.errorMessages[nameControl][rule];
  }

  // if (MESSAGE_VALIDATION[nameControl] && MESSAGE_VALIDATION[nameControl][rule]) {
  //   return  MESSAGE_VALIDATION[nameControl][rule];
  // }

  // by rule
  if (MESSAGE_VALIDATION_RULE[rule]) {
    return  MESSAGE_VALIDATION_RULE[rule];
  }
  return 'chưa định nghĩa msg: ' + nameControl + '|' + rule;
}

getNameControl() {
  if (this.formControlName) {
    return this.formControlName;
  }
  var controlName = null;
  var parent = this.formControl.parent;
  // only such parent, which is FormGroup, has a dictionary
  // with control-names as a key and a form-control as a value
  if (parent instanceof FormGroup) {
    for (const key of Object.keys(parent.controls)) {
      if (this.formControl === parent.controls[key]) {
          // both are same: control passed to Validator
          //  and this child - are the same references
          controlName = key;
          break;
      }
    }
  }
  // we either found a name or simply return null
  return controlName;
}

ngOnDestroy(): void {
  if (this._entry) {
    this._entry.clear();
  }
}

  refresh(){
    (<InputTypeBase<any>>this.componentRef.instance).refresh();
  }
}
