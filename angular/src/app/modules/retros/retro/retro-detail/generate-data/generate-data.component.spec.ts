import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GenerateDataComponent } from './generate-data.component';

describe('GenerateDataComponent', () => {
  let component: GenerateDataComponent;
  let fixture: ComponentFixture<GenerateDataComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GenerateDataComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GenerateDataComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
