import { Injectable } from "@angular/core";
import { HttpClient } from "@microsoft/signalr";
import { asyncScheduler, BehaviorSubject, observeOn } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class LanguageService {

    private languageBSubject = new BehaviorSubject<boolean>(false);

    switchLanguage() {

        const currentValue = this.languageBSubject.getValue();

        this.languageBSubject.next(!currentValue);

        localStorage.setItem('tmL', JSON.stringify(!currentValue));

    }

    get isSpanish$() {
        return this.languageBSubject.asObservable()
    }

    constructor() {

        const languageLS = localStorage.getItem('tmL');

        if (languageLS) {

            let isSpanish = JSON.parse(languageLS);

            this.languageBSubject.next(isSpanish);
        }
    }
}