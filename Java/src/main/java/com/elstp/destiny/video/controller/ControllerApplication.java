package com.elstp.destiny.video.controller;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class ControllerApplication {

    public static void main(String[] args) {
        try{
            SpringApplication.run(ControllerApplication.class, args);
            System.out.println("========ELSTP STUDIO========");
        }catch (Exception e){
            e.printStackTrace();
        }
    }

}
