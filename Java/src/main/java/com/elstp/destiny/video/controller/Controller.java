package com.elstp.destiny.video.controller;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONArray;
import com.alibaba.fastjson.JSONObject;
import com.elstp.destiny.video.controller.Utils.HttpClientUtils;
import org.springframework.util.StringUtils;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class Controller {


    @RequestMapping("/biliBili")
    public String biliBili (String avId){
        if (StringUtils.isEmpty(avId)) {
            return "";
        }
        if ("av".equalsIgnoreCase(avId.substring(0,2))) {
            avId = avId.substring(2,avId.length());
        }

      String data =   HttpClientUtils.sendGetRequest("https://api.bilibili.com/x/web-interface/view", "aid="+avId);
        JSONObject jsonObject = JSON.parseObject(data);
        if (0 != jsonObject.getInteger("code")) {
            return "";
        }
       JSONObject re  =  jsonObject.getJSONObject("data");
      Integer cId =  re.getInteger("cid");
      String  title  =  re.getString("title");
      String data2 =   HttpClientUtils.sendGetRequest("https://api.bilibili.com/x/player/playurl", "avid="+avId+"&cid="+cId+"&qn=80&type=&fnver=0&fnval=16&otype=json");
        JSONObject jsonObject2 = JSON.parseObject(data2);
        if (0 != jsonObject2.getInteger("code")) {
            return "";
        }
        JSONObject re2 = jsonObject2.getJSONObject("data");
        //得到视频
        JSONArray jsonVideoArray = re2.getJSONArray("durl");
       String VideoBaseUrl =  jsonVideoArray.getJSONObject(0).getString("url");
    return  VideoBaseUrl;
    }


    @RequestMapping("/getTest")
    public String getTest (String str){

        return  "YOU:"+str;
    }



}
