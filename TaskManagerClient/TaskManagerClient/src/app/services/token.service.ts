import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { HttpClient } from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})
export class TokenService { 

    getNewAccessToken(refreshToken:string){
        return this.http.post<{accessToken:string, refreshToken:string}>
        (`${environment.tokensUrl}/GetAccessToken/${refreshToken}`, null)
    }

    constructor(private http:HttpClient){}

}