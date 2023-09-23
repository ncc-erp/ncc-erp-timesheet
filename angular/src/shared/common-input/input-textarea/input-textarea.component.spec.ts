import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InputTextareaComponent } from './input-textarea.component';

describe('InputTextareaComponent', () => {
  let component: InputTextareaComponent;
  let fixture: ComponentFixture<InputTextareaComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InputTextareaComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InputTextareaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
