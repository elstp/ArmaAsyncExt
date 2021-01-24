package com.elstp.destiny.video.controller;

import com.sun.xml.internal.ws.api.message.Packet;

import java.io.IOException;
import java.net.InetAddress;
import java.net.NetworkInterface;
import java.net.SocketException;

public class test {
    public static void main(String[] args) {
               //1枚举网卡并打开设备
  jpcap.NetworkInterface[] devices = JpcapCaptor.getDeviceList();
  NetworkInterface device = devices[2];0
  JpcapSender sender = null;
  //2.原始类型数据包，这种包没有首部字节。UDPPacket、IPPacket有首部字节
        //需要设置udp.setIPv4Parameter(0,false,false,false,0,false,false,false,0,0,255,
        //   230,//230未定义协议
        //   InetAddress.getByName("192.168.1.100"), 
        //   InetAddress.getByName("192.168.1.102"));
  Packet udp = new Packet();
  try {
      sender = JpcapSender.openDevice(device);
  } catch (IOException e) {
   e.printStackTrace();
  }
  EthernetPacket ether = new EthernetPacket();
  //设置自定义协议类型
  int type = Integer.decode("0x7799");
  ether.frametype = (short)type;
  byte[] desmac = stomac(macAddress);//目标mac地址
  byte[] srcmac = null;
  try {
   srcmac = stomac(getLocalMac(InetAddress.getLocalHost()));
  } catch (SocketException e) {
   e.printStackTrace();
  } catch (UnknownHostException e) {
   e.printStackTrace();
  } // 本机MAC数组
  ether.src_mac = srcmac;
  ether.dst_mac = desmac;
  udp.datalink = ether;
  //设置字节的数据以及长度
   byte[] data = new byte[54];
      String typeStr = "hello";
         byte[] types = ByteUtil.ToByte(typeStr);
         for (int i = 0; i < 2; i++) {
         data[i] = types[i];
         }
   udp.data = data;
   sender.sendPacket(udp);

    }
}
