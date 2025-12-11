import { Component, ElementRef, HostListener, Injector, OnInit } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { ConfigurationServiceProxy, ChangeUiThemeInput } from '@shared/service-proxies/service-proxies';

class UiThemeInfo {
    constructor(
        public name: string,
        public cssClass: string,
        public icon: string = 'palette'
    ) { }
}

@Component({
    selector: 'theme-selector',
    templateUrl: './theme-selector.component.html',
    styleUrls: ['./theme-selector.component.css']
})
export class ThemeSelectorComponent extends AppComponentBase implements OnInit {

    themes: UiThemeInfo[] = [
        new UiThemeInfo('Light', 'light', 'light_mode'),
        new UiThemeInfo('Dark', 'dark', 'dark_mode'),
        new UiThemeInfo('Red', 'red', 'palette'),
        new UiThemeInfo('Pink', 'pink', 'palette'),
        new UiThemeInfo('Purple', 'purple', 'palette'),
        new UiThemeInfo('Deep Purple', 'deep-purple', 'palette'),
        new UiThemeInfo('Indigo', 'indigo', 'palette'),
        new UiThemeInfo('Blue', 'blue', 'palette'),
        new UiThemeInfo('Light Blue', 'light-blue', 'palette'),
        new UiThemeInfo('Cyan', 'cyan', 'palette'),
        new UiThemeInfo('Teal', 'teal', 'palette'),
        new UiThemeInfo('Green', 'green', 'palette'),
        new UiThemeInfo('Light Green', 'light-green', 'palette'),
        new UiThemeInfo('Lime', 'lime', 'palette'),
        new UiThemeInfo('Yellow', 'yellow', 'palette'),
        new UiThemeInfo('Amber', 'amber', 'palette'),
        new UiThemeInfo('Orange', 'orange', 'palette'),
        new UiThemeInfo('Deep Orange', 'deep-orange', 'palette'),
        new UiThemeInfo('Brown', 'brown', 'palette'),
        new UiThemeInfo('Grey', 'grey', 'palette'),
        new UiThemeInfo('Blue Grey', 'blue-grey', 'palette'),
        new UiThemeInfo('Black', 'black', 'palette')
    ];

    selectedTheme: UiThemeInfo;
    isThemeMenuOpen = false;

    constructor(
        injector: Injector,
        private _configurationService: ConfigurationServiceProxy,
        private hostElement: ElementRef,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        const currentTheme = this.setting.get('App.UiTheme') || 'red';
        this.selectedTheme = this.themes.find(t => t.cssClass === currentTheme) || this.themes[0];
        $('body').addClass('theme-' + this.selectedTheme.cssClass);
    }

    toggleThemeMenu(): void {
        this.isThemeMenuOpen = !this.isThemeMenuOpen;
    }

    setTheme(theme: UiThemeInfo): void {
        const input = new ChangeUiThemeInput();
        input.theme = theme.cssClass;

        this._configurationService.changeUiTheme(input).subscribe(() => {
            const $body = $('body');

            $body.removeClass('theme-' + this.selectedTheme.cssClass);

            $body.addClass('theme-' + theme.cssClass);

            this.selectedTheme = theme;
            this.isThemeMenuOpen = false;
        });
    }

    @HostListener('document:click', ['$event'])
    onDocumentClick(event: MouseEvent): void {
        if (!this.isThemeMenuOpen) {
            return;
        }

        const target = event.target as HTMLElement | null;
        if (!target) {
            return;
        }

        const hostNativeElement = this.hostElement.nativeElement as HTMLElement;
        if (!hostNativeElement.contains(target)) {
            this.isThemeMenuOpen = false;
        }
    }

    getThemeColor(theme: UiThemeInfo): string {
        const colorMap: { [key: string]: string } = {
            'light': '#f8f9fa',
            'dark': '#212529',
            'red': '#f44336',
            'pink': '#e91e63',
            'purple': '#9c27b0',
            'deep-purple': '#673ab7',
            'indigo': '#3f51b5',
            'blue': '#2196f3',
            'light-blue': '#03a9f4',
            'cyan': '#00bcd4',
            'teal': '#009688',
            'green': '#4caf50',
            'light-green': '#8bc34a',
            'lime': '#cddc39',
            'yellow': '#ffeb3b',
            'amber': '#ffc107',
            'orange': '#ff9800',
            'deep-orange': '#ff5722',
            'brown': '#795548',
            'grey': '#9e9e9e',
            'blue-grey': '#607d8b',
            'black': '#000000'
        };
        return colorMap[theme.cssClass] || '#2196f3';
    }
}