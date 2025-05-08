# Metin2toUnity Map Tool Scripts

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2020.3%2B-blue" alt="Unity Version">
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License">
  <img src="https://img.shields.io/badge/Metin2-Map%20Tools-orange" alt="Metin2 Map Tools">
</p>

A comprehensive set of Unity Editor tools designed to help convert, import, and optimize Metin2 game maps for Unity projects.

[English](#english-documentation) | [Türkçe](#türkçe-dokümantasyon)

---

## English Documentation

### Overview

This collection of Unity Editor tools provides everything you need to import and optimize Metin2 maps in Unity. The toolkit includes four specialized tools designed to handle different aspects of the conversion process:

1. **Metin2 Map Builds Importer** - Import objects and structures from Metin2 maps
2. **Metin2 Terrain Tile Importer** - Import and process terrain heightmaps and textures
3. **Metin2 Terrain Edge Fixer** - Fix seams and edges between terrain tiles
4. **Metin2 Terrain Layer Transfer Tool** - Transfer terrain layers between terrain tiles

### Installation

1. Make sure you have Unity 2020.3 or newer installed
2. Download or clone this repository
3. Copy the folders into your Unity project's `Assets` directory
4. The tools will be available in the Unity Editor under the "Tools" menu

### Requirements

- Unity 2020.3 or newer
- Basic knowledge of Unity terrain system
- Metin2 map files (.raw heightmaps, texture sets, etc.)

### Tools Description

#### 1. Metin2 Map Builds Importer

This tool allows you to import objects and structures from Metin2 map files into Unity.

**Features:**
- Import map objects from Metin2 property files
- Search and match models by property name
- Place objects according to original coordinates
- Snap objects to terrain

**Usage:**
1. Open the tool via `Tools > Metin2 Map Builds Importer - @Metin2Avi`
2. Select the source directory containing Metin2 map files
3. Configure import settings
4. Click "Import" to process the files

#### 2. Metin2 Terrain Tile Importer

This tool imports terrain heightmaps and textures from Metin2 into Unity terrain objects.

**Features:**
- Import raw heightmap files
- Apply texture sets to terrain
- Batch processing for multiple terrain tiles
- Coordinate-based mapping for accurate positioning

**Usage:**
1. Open the tool via `Tools > Metin2 Terrain Tile Importer - @Metin2Avi`
2. Select a raw heightmap file or configure batch processing
3. Set the texture directory
4. Adjust import settings (flip options, blend settings)
5. Click "Import" to process the terrain data

#### 3. Metin2 Terrain Edge Fixer

This tool fixes the common seam issues between adjacent terrain tiles in Unity.

**Features:**
- Blend heightmaps between adjacent terrains
- Smooth texture transitions
- Adjustable blend distance and settings

**Usage:**
1. Open the tool via `Tools > Metin2 Terrain Edge Fixer - @Metin2Avi`
2. Select the terrains you want to fix
3. Adjust blend settings
4. Click "Fix Edges" to process

#### 4. Metin2 Terrain Layer Transfer Tool

This tool allows you to transfer terrain texture layers from one terrain to multiple others.

**Features:**
- Copy all terrain layers from a source terrain
- Apply to multiple target terrains at once
- Maintain consistent terrain appearance

**Usage:**
1. Open the tool via `Tools > Metin2 Terrain Layer Transfer - @Metin2Avi`
2. Select a source terrain with the desired layers
3. Add target terrains to transfer to
4. Click "Transfer Layers" to process

### Advanced Usage

#### Map Coordinate System

The tools use a coordinate-based system to ensure proper alignment of terrain tiles and objects. Make sure your terrains are properly named and positioned according to their Metin2 map coordinates for best results.

#### Batch Processing

For larger maps, the Terrain Tile Importer offers batch processing to import multiple terrain tiles at once:

1. Enable "Batch Process Mode"
2. Set the base directory containing multiple raw files
3. Configure terrain mapping
4. Click "Process All" to import multiple terrain tiles

### Troubleshooting

**Common Issues:**
- If objects appear floating, use the "Snap to Ground" option
- For missing textures, ensure the texture path is correctly set
- If terrain edges still have seams after fixing, try increasing the blend distance

---

## Türkçe Dokümantasyon

### Genel Bakış

Bu Unity Editor araçları koleksiyonu, Metin2 haritalarını Unity'ye aktarmak ve optimize etmek için gereken her şeyi sağlar. Araç seti, dönüştürme sürecinin farklı yönlerini ele almak üzere tasarlanmış dört özel araç içerir:

1. **Metin2 Harita Binaları İçe Aktarıcı** - Metin2 haritalarından nesneleri ve yapıları içe aktarma
2. **Metin2 Arazi Parçası İçe Aktarıcı** - Arazi yükseklik haritalarını ve dokularını içe aktarma ve işleme
3. **Metin2 Arazi Kenar Düzeltici** - Arazi parçaları arasındaki dikiş yerlerini ve kenarları düzeltme
4. **Metin2 Arazi Katmanı Transfer Aracı** - Arazi parçaları arasında arazi katmanlarını aktarma

### Kurulum

1. Unity 2020.3 veya daha yeni bir sürümünün kurulu olduğundan emin olun
2. Bu depoyu indirin veya klonlayın
3. Klasörleri Unity projenizin `Assets` dizinine kopyalayın
4. Araçlar, Unity Editor'de "Tools" menüsü altında kullanılabilir olacaktır

### Gereksinimler

- Unity 2020.3 veya daha yeni
- Unity arazi sistemi hakkında temel bilgi
- Metin2 harita dosyaları (.raw yükseklik haritaları, doku setleri, vb.)

### Araçların Açıklaması

#### 1. Metin2 Harita Binaları İçe Aktarıcı

Bu araç, Metin2 harita dosyalarından nesneleri ve yapıları Unity'ye aktarmanızı sağlar.

**Özellikler:**
- Metin2 property dosyalarından harita nesnelerini içe aktarma
- Property adına göre modelleri arama ve eşleştirme
- Nesneleri orijinal koordinatlara göre yerleştirme
- Nesneleri araziye yapıştırma

**Kullanım:**
1. Aracı `Tools > Metin2 Map Builds Importer - @Metin2Avi` yoluyla açın
2. Metin2 harita dosyalarını içeren kaynak dizini seçin
3. İçe aktarma ayarlarını yapılandırın
4. Dosyaları işlemek için "Import" düğmesine tıklayın

#### 2. Metin2 Arazi Parçası İçe Aktarıcı

Bu araç, Metin2'den arazi yükseklik haritalarını ve dokularını Unity arazi nesnelerine aktarır.

**Özellikler:**
- Raw yükseklik harita dosyalarını içe aktarma
- Araziye doku setleri uygulama
- Birden fazla arazi parçası için toplu işleme
- Doğru konumlandırma için koordinat tabanlı haritalama

**Kullanım:**
1. Aracı `Tools > Metin2 Terrain Tile Importer - @Metin2Avi` yoluyla açın
2. Bir raw yükseklik harita dosyası seçin veya toplu işlemeyi yapılandırın
3. Doku dizinini ayarlayın
4. İçe aktarma ayarlarını yapın (çevirme seçenekleri, karıştırma ayarları)
5. Arazi verilerini işlemek için "Import" düğmesine tıklayın

#### 3. Metin2 Arazi Kenar Düzeltici

Bu araç, Unity'de bitişik arazi parçaları arasındaki yaygın dikiş sorunlarını giderir.

**Özellikler:**
- Bitişik araziler arasında yükseklik haritalarını harmanlama
- Doku geçişlerini yumuşatma
- Ayarlanabilir harmanlama mesafesi ve ayarları

**Kullanım:**
1. Aracı `Tools > Metin2 Terrain Edge Fixer - @Metin2Avi` yoluyla açın
2. Düzeltmek istediğiniz arazileri seçin
3. Harmanlama ayarlarını yapın
4. İşlemek için "Fix Edges" düğmesine tıklayın

#### 4. Metin2 Arazi Katmanı Transfer Aracı

Bu araç, arazi doku katmanlarını bir araziden diğerlerine aktarmanızı sağlar.

**Özellikler:**
- Kaynak araziden tüm arazi katmanlarını kopyalama
- Birden fazla hedef araziye aynı anda uygulama
- Tutarlı arazi görünümünü koruma

**Kullanım:**
1. Aracı `Tools > Metin2 Terrain Layer Transfer - @Metin2Avi` yoluyla açın
2. İstenen katmanlara sahip bir kaynak arazi seçin
3. Aktarılacak hedef arazileri ekleyin
4. İşlemek için "Transfer Layers" düğmesine tıklayın

### Gelişmiş Kullanım

#### Harita Koordinat Sistemi

Araçlar, arazi parçalarının ve nesnelerin düzgün hizalanmasını sağlamak için koordinat tabanlı bir sistem kullanır. En iyi sonuçlar için arazilerinizin, Metin2 harita koordinatlarına göre uygun şekilde adlandırıldığından ve konumlandırıldığından emin olun.

#### Toplu İşleme

Daha büyük haritalar için, Arazi Parçası İçe Aktarıcı, birden fazla arazi parçasını bir kerede içe aktarmak için toplu işleme sunar:

1. "Batch Process Mode" seçeneğini etkinleştirin
2. Birden fazla raw dosyası içeren temel dizini ayarlayın
3. Arazi haritalandırmasını yapılandırın
4. Birden fazla arazi parçasını içe aktarmak için "Process All" düğmesine tıklayın

### Sorun Giderme

**Yaygın Sorunlar:**
- Nesneler havada görünüyorsa, "Snap to Ground" seçeneğini kullanın
- Eksik dokular için, doku yolunun doğru ayarlandığından emin olun
- Arazi kenarları düzeltildikten sonra hala dikiş yerleri varsa, harmanlama mesafesini artırmayı deneyin

---

## Author

This toolkit is developed by Metin2Avi

### Social Links

- Instagram: [@metin2.avi](https://www.instagram.com/metin2.avi/)
- Discord: [Join Server](https://discord.gg/WZMzMgPp38)
- YouTube: [@project_avi](https://www.youtube.com/@project_avi)
- Metin2Downloads: [Metin2Avi](https://www.metin2downloads.to/cms/user/30621-metin2avi/)
- M2Dev: [Metin2Avi](https://metin2.dev/profile/53064-metin2avi/)
- TurkmmoForum: [trmove](https://forum.turkmmo.com/uye/165187-trmove/)

## License

This project is licensed under the MIT License - see the LICENSE file for details.
