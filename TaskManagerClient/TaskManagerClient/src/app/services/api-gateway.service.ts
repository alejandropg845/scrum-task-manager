import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";

@Injectable({
    providedIn: 'root'
})
export class ApiGatewayService { 

    sendRequest<T>(method:string, microservice:string, endpoint:string, body?:any, isBlobResponse?: boolean){
    
        var headers = new HttpHeaders()
        .set("method", method)
        .set("microservice", microservice)
        .set("endpoint", endpoint);

        return this.http.post<T>(`${environment.api_gateway}`, body ?? null, { headers, responseType: !isBlobResponse ? 'json' : 'blob' as 'json'})

    }

    constructor(private http:HttpClient){}

}