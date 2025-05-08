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
- Original Metin2 game assets (for full functionality)

### Tools Description

#### 1. Metin2 Map Builds Importer

This tool allows you to import objects and structures from Metin2 map files into Unity with precise positioning and properties.

**Features:**
- Import map objects from Metin2 property files (.prt and .prb files)
- Intelligently search and match models by property name and ID
- Automatically place objects according to original coordinates with configurable offset
- Snap objects to terrain height during import with adjustable ground offset
- Fix tree rotation automatically with -90° X-axis rotation option
- Create ScriptableObjects to store imported object data for future reference
- Support for file caching to improve performance with large maps
- Coordinate system conversion with flip options for X and Z coordinates

**Technical Details:**
- Uses `IMetin2ObjectData` interface and `ObjectDataWrapper` class for serialized data management
- Creates organized collections with `Metin2ObjectsCollection` class for each sector and object type
- Preprocesses property files with the "Pre-Scan Property Files" feature for faster large-scale imports
- Supports creating primitive objects as fallbacks when original models aren't found

**Usage:**
1. Open the tool via `Tools > Metin2 Map Builds Importer - @Metin2Avi`
2. Assign a target terrain for height referencing
3. Set the map folder path containing areadata files 
4. Set the property folder path containing .prt and .prb files
5. (Optional) Configure Unity models folder path for matching FBX files
6. Adjust coordinate flipping options (typically Z-flip is enabled by default)
7. Set map offset and ground snap options
8. Click "Pre-Scan Property Files" for large maps to improve import speed
9. Click "Import All Objects" to process the files

#### 2. Metin2 Terrain Tile Importer

This tool imports terrain heightmaps and textures from Metin2 map files (.raw) into Unity terrain objects with comprehensive texture set support.

**Features:**
- Import raw heightmap files (.raw) with proper coordinate mapping
- Support for both standard mode (single terrain) and batch processing (multiple terrains)
- Apply texture sets to terrains with automatic texture detection
- Coordinate-based mapping with full support for 6-digit map coordinate system (XXXYYY format)
- Custom drag-and-drop terrain ordering interface for precise control
- Multiple texture set formats supported (standard, generic, and custom)
- Automatic detection of texture set files
- Blend radius and sharpness controls for smoother terrain transitions

**Technical Details:**
- Supports multiple texture set file formats and naming conventions
- Creates appropriate Unity TerrainLayers from texture sets
- Generates splatmaps from raw data with proper coordinate handling
- Provides complete batch processing workflow for large-scale map imports
- Includes automatic map area scanning with proper sorting

**Usage:**
1. Open the tool via `Tools > Metin2 Terrain Tile Importer - @Metin2Avi`
2. For single terrain mode:
   - Select a raw heightmap file
   - Set texture set path
   - Configure import settings
   - Click "Import" to process
3. For batch processing mode:
   - Enable "Batch Process Mode"
   - Set the base directory containing multiple map areas (with 6-digit folder names)
   - Use the terrain mapping interface to associate terrains with map coordinates
   - Configure global settings
   - Click "Process All Terrains" to import all map areas

#### 3. Metin2 Terrain Edge Fixer

This tool fixes the common seam issues between adjacent terrain tiles in Unity by blending heightmaps and textures at terrain edges.

**Features:**
- Blend heightmaps between adjacent terrains to eliminate visible seams
- Smooth texture transitions between terrain tiles
- Adjustable blend distance to control the blending effect strength
- Separate toggles for heightmap and texture blending
- Automatic neighbor detection based on terrain positions
- Undo support for all terrain modifications

**Technical Details:**
- Uses intelligent neighbor detection to find adjacent terrains
- Implements gradual height blending at edges with adjustable falloff
- Normalizes alphamap row values to maintain texture balance
- Preserves terrain data integrity with proper Unity serialization
- Optimized algorithms to minimize processing time

**Usage:**
1. Open the tool via `Tools > Metin2 Terrain Edge Fixer - @Metin2Avi`
2. Select multiple terrains in the Unity scene
3. Click "Get Selected Terrains" to add them to the tool
4. Adjust the blend distance slider (higher values create smoother but wider transitions)
5. Toggle heightmap and texture blending options as needed
6. Click "Fix Edges" to process the selected terrains

#### 4. Metin2 Terrain Layer Transfer Tool

This tool allows you to transfer terrain texture layers from one terrain to multiple target terrains, ensuring consistent appearance across your map.

**Features:**
- Copy all terrain layers from a source terrain to multiple target terrains
- Maintain consistent terrain layer settings across the entire map
- Reorderable list interface for managing target terrains
- Undo support for safe experimentation
- One-click operation for transferring to all targets simultaneously

**Technical Details:**
- Preserves all layer settings including normal maps, metallic values, and smoothness
- Uses Unity's serialization system to ensure proper data handling
- Implements UnityEditorInternal.ReorderableList for better UI experience
- Properly registers Undo operations for each terrain modification

**Usage:**
1. Open the tool via `Tools > Metin2 Terrain Layer Transfer - @Metin2Avi`
2. Select a source terrain that contains the desired terrain layers
3. Add target terrains using the list interface (can be individually added and reordered)
4. Click "Transfer Layers to All Targets" to copy the layers to all target terrains

### Advanced Usage

#### Map Coordinate System

The tools use Metin2's coordinate-based system to ensure proper alignment of terrain tiles and objects:

- Metin2 maps use a 6-digit coordinate system for areas (XXXYYY format)
- The batch processing in TerrainTileImporter automates the mapping between these coordinates and Unity terrains
- The MapBuildsImporter includes map offset options to fine-tune positioning

#### Texture Set Format

The TerrainTileImporter supports multiple texture set formats:

1. **Standard Format**: Uses the classic pattern with each line containing texture path and detailing
2. **Generic Format**: Simpler format with just texture paths
3. **Default Fallback**: Automatically generated when no texture set is found

Example of Standard TextureSet.txt format:
```
d:/ymir work/terrain/metin2_a1/field.dds 0 0
d:/ymir work/terrain/metin2_a1/stone.dds 0.8 0
...
```

#### Model Naming Conventions

The MapBuildsImporter uses property names and IDs to match with Unity models. For best results:
- Name your models to match Metin2 property names
- Place models in a consistent folder structure that the tool can search
- Use the "Pre-Scan" feature to build a complete property database before importing

### Troubleshooting

**Common Issues:**

#### Map Builds Importer
- **Objects appear floating**: Enable "Snap to Ground" and adjust the ground offset value
- **Missing models**: Check the Unity models folder path and verify your model names match property names
- **Incorrect positioning**: Verify the correct flip settings (typically Z needs to be flipped) and adjust map offset values
- **Import is slow**: Use the "Pre-Scan Property Files" feature to build a cache before importing

#### Terrain Tile Importer
- **Texture issues**: Ensure texture paths in texture sets are correctly mapped to Unity project assets
- **Incorrect terrain order**: Use the drag-and-drop interface to manually set the terrain order
- **Missing heightmaps**: Verify that raw files are in the expected 6-digit coordinate folders

#### Terrain Edge Fixer
- **Visible seams remain**: Increase the blend distance value for smoother transitions
- **Texture blending issues**: Ensure terrains share compatible terrain layers

#### Layer Transfer Tool
- **Transfer not working**: Verify both source and target terrains have valid TerrainData assets
- **Layers not showing after transfer**: Ensure the terrains have proper alphamaps/splatmaps assigned

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
- Orijinal Metin2 oyun varlıkları (tam işlevsellik için)

### Araçların Açıklaması

#### 1. Metin2 Harita Binaları İçe Aktarıcı

Bu araç, Metin2 harita dosyalarından nesneleri ve yapıları hassas konumlandırma ve özelliklerle Unity'ye aktarmanızı sağlar.

**Özellikler:**
- Metin2 property dosyalarından (.prt ve .prb dosyaları) harita nesnelerini içe aktarma
- Akıllıca property adı ve ID'ye göre modelleri arama ve eşleştirme
- Orijinal koordinatlara göre nesneleri ayarlanabilir offset ile otomatik yerleştirme
- İçe aktarma sırasında nesneleri ayarlanabilir zemin offset'i ile araziye otomatik yapıştırma
- Ağaç rotasyonunu -90° X-ekseni rotasyon seçeneği ile otomatik düzeltme
- İçe aktarılan nesne verilerini ileride kullanmak üzere ScriptableObjects oluşturma
- Büyük haritalarda performansı artırmak için dosya önbelleğe alma desteği
- X ve Z koordinatları için çevirme seçenekleri ile koordinat sistemi dönüşümü

**Teknik Detaylar:**
- Serileştirilmiş veri yönetimi için `IMetin2ObjectData` arayüzü ve `ObjectDataWrapper` sınıfını kullanır
- Her sektör ve nesne tipi için `Metin2ObjectsCollection` sınıfı ile düzenli koleksiyonlar oluşturur
- Büyük ölçekli ithalatlar için daha hızlı "Pre-Scan Property Files" özelliği ile property dosyalarını önişleme tabi tutar
- Orijinal modeller bulunamadığında yedek olarak ilkel nesneler oluşturulmasını destekler

**Kullanım:**
1. Aracı `Tools > Metin2 Map Builds Importer - @Metin2Avi` yoluyla açın
2. Yükseklik referansı için bir hedef arazi atayın
3. Areadata dosyalarını içeren harita klasör yolunu ayarlayın
4. .prt ve .prb dosyalarını içeren property klasör yolunu ayarlayın
5. (İsteğe bağlı) FBX dosyalarını eşleştirmek için Unity modelleri klasör yolunu yapılandırın
6. Koordinat çevirme seçeneklerini ayarlayın (genellikle Z-çevirme varsayılan olarak etkindir)
7. Harita offset ve zemin yapışma seçeneklerini ayarlayın
8. İçe aktarma hızını artırmak için büyük haritalar için "Pre-Scan Property Files" düğmesine tıklayın
9. Dosyaları işlemek için "Import All Objects" düğmesine tıklayın

#### 2. Metin2 Arazi Parçası İçe Aktarıcı

Bu araç, Metin2 harita dosyalarındaki (.raw) arazi yükseklik haritalarını ve dokularını kapsamlı doku seti desteğiyle Unity arazi nesnelerine aktarır.

**Özellikler:**
- Raw yükseklik harita dosyalarını (.raw) doğru koordinat eşlemesi ile içe aktarma
- Hem standart mod (tek arazi) hem de toplu işleme (birden çok arazi) desteği
- Otomatik doku algılama ile arazilere doku setleri uygulama
- 6 basamaklı harita koordinat sistemi (XXXYYY formatı) için tam destekli koordinat tabanlı haritalama
- Hassas kontrol için özel sürükle ve bırak arazi sıralama arayüzü
- Desteklenen birden fazla doku seti formatı (standart, genel ve özel)
- Doku seti dosyalarının otomatik algılanması
- Daha pürüzsüz arazi geçişleri için karıştırma yarıçapı ve keskinlik kontrolleri

**Teknik Detaylar:**
- Birden fazla doku seti dosya formatını ve adlandırma kurallarını destekler
- Doku setlerinden uygun Unity TerrainLayers oluşturur
- Raw verilerden uygun koordinat işleme ile splatmap'ler oluşturur
- Büyük ölçekli harita içe aktarmaları için eksiksiz toplu işleme iş akışı sağlar
- Uygun sıralama ile otomatik harita alanı taraması içerir

**Kullanım:**
1. Aracı `Tools > Metin2 Terrain Tile Importer - @Metin2Avi` yoluyla açın
2. Tek arazi modu için:
   - Bir raw yükseklik harita dosyası seçin
   - Doku seti yolunu ayarlayın
   - İçe aktarma ayarlarını yapılandırın
   - İşlemek için "Import" düğmesine tıklayın
3. Toplu işleme modu için:
   - "Batch Process Mode" seçeneğini etkinleştirin
   - Birden fazla harita alanı içeren temel dizini ayarlayın (6 basamaklı klasör adlarıyla)
   - Arazileri harita koordinatlarıyla ilişkilendirmek için arazi haritalama arayüzünü kullanın
   - Global ayarları yapılandırın
   - Tüm harita alanlarını içe aktarmak için "Process All Terrains" düğmesine tıklayın

#### 3. Metin2 Arazi Kenar Düzeltici

Bu araç, yükseklik haritalarını ve arazilerin kenarlarındaki dokuları harmanlayarak Unity'deki bitişik arazi karoları arasındaki yaygın dikiş sorunlarını giderir.

**Özellikler:**
- Görünür dikişleri ortadan kaldırmak için bitişik araziler arasında yükseklik haritalarını harmanlama
- Arazi karoları arasında pürüzsüz doku geçişleri
- Harmanlama efekti gücünü kontrol etmek için ayarlanabilir harmanlama mesafesi
- Yükseklik haritası ve doku harmanlaması için ayrı açma/kapama düğmeleri
- Arazi konumlarına dayalı otomatik komşu algılama
- Tüm arazi değişiklikleri için geri alma desteği

**Teknik Detaylar:**
- Bitişik arazileri bulmak için akıllı komşu algılama kullanır
- Ayarlanabilir azalma ile kenarlarda kademeli yükseklik harmanlaması uygular
- Doku dengesini korumak için alfamap satır değerlerini normalleştirir
- Uygun Unity serileştirme ile arazi veri bütünlüğünü korur
- İşlem süresini en aza indirmek için optimize edilmiş algoritmalar

**Kullanım:**
1. Aracı `Tools > Metin2 Terrain Edge Fixer - @Metin2Avi` yoluyla açın
2. Unity sahnesinde birden fazla arazi seçin
3. Bunları araca eklemek için "Get Selected Terrains" düğmesine tıklayın
4. Harmanlama mesafesi kaydırıcısını ayarlayın (daha yüksek değerler daha pürüzsüz ancak daha geniş geçişler oluşturur)
5. Gerektiğinde yükseklik haritası ve doku harmanlama seçeneklerini değiştirin
6. Seçilen arazileri işlemek için "Fix Edges" düğmesine tıklayın

#### 4. Metin2 Arazi Katmanı Transfer Aracı

Bu araç, arazi doku katmanlarını bir araziden birden çok hedef araziye aktarmanıza olanak tanıyarak haritanızın tamamında tutarlı bir görünüm sağlar.

**Özellikler:**
- Tüm arazi katmanlarını kaynak araziden birden çok hedef araziye kopyalama
- Tüm harita boyunca tutarlı arazi katmanı ayarlarını koruma
- Hedef arazileri yönetmek için yeniden sıralanabilir liste arayüzü
- Güvenli denemeler için geri alma desteği
- Tüm hedeflere aynı anda aktarmak için tek tıklamalı işlem

**Teknik Detaylar:**
- Normal haritalar, metalik değerler ve pürüzsüzlük dahil tüm katman ayarlarını korur
- Uygun veri işleme için Unity'nin serileştirme sistemini kullanır
- Daha iyi kullanıcı arayüzü deneyimi için UnityEditorInternal.ReorderableList uygular
- Her arazi değişikliği için uygun şekilde Geri Al işlemlerini kaydeder

**Kullanım:**
1. Aracı `Tools > Metin2 Terrain Layer Transfer - @Metin2Avi` yoluyla açın
2. İstenen arazi katmanlarını içeren bir kaynak arazi seçin
3. Liste arayüzünü kullanarak hedef arazileri ekleyin (tek tek eklenebilir ve yeniden sıralanabilir)
4. Katmanları tüm hedef arazilere kopyalamak için "Transfer Layers to All Targets" düğmesine tıklayın

### Gelişmiş Kullanım

#### Harita Koordinat Sistemi

Araçlar, arazi karolarının ve nesnelerin düzgün hizalanmasını sağlamak için Metin2'nin koordinat tabanlı sistemini kullanır:

- Metin2 haritaları alanlar için 6 basamaklı bir koordinat sistemi kullanır (XXXYYY formatı)
- TerrainTileImporter'daki toplu işleme, bu koordinatlar ile Unity arazileri arasındaki haritalamayı otomatikleştirir
- MapBuildsImporter, konumlandırmayı ince ayarlamak için harita offset seçenekleri içerir

#### Doku Seti Formatı

TerrainTileImporter birden fazla doku seti formatını destekler:

1. **Standart Format**: Her satırda doku yolu ve detaylandırma içeren klasik deseni kullanır
2. **Genel Format**: Sadece doku yolları içeren daha basit format
3. **Varsayılan Yedek**: Doku seti bulunamadığında otomatik olarak oluşturulur

Standart TextureSet.txt formatı örneği:
```
d:/ymir work/terrain/metin2_a1/field.dds 0 0
d:/ymir work/terrain/metin2_a1/stone.dds 0.8 0
...
```

#### Model Adlandırma Kuralları

MapBuildsImporter, Unity modelleriyle eşleştirmek için property adlarını ve ID'leri kullanır. En iyi sonuçlar için:
- Modellerinizi Metin2 property adlarıyla eşleşecek şekilde adlandırın
- Modelleri aracın arayabileceği tutarlı bir klasör yapısına yerleştirin
- İçe aktarmadan önce eksiksiz bir property veritabanı oluşturmak için "Pre-Scan" özelliğini kullanın

### Sorun Giderme

**Yaygın Sorunlar:**

#### Harita Binaları İçe Aktarıcı
- **Nesneler havada görünüyor**: "Snap to Ground" seçeneğini etkinleştirin ve zemin offset değerini ayarlayın
- **Eksik modeller**: Unity modelleri klasör yolunu kontrol edin ve model adlarınızın property adlarıyla eşleştiğini doğrulayın
- **Yanlış konumlandırma**: Doğru çevirme ayarlarını doğrulayın (genellikle Z'nin çevrilmesi gerekir) ve harita offset değerlerini ayarlayın
- **İçe aktarma yavaş**: İçe aktarmadan önce önbellek oluşturmak için "Pre-Scan Property Files" özelliğini kullanın

#### Arazi Parçası İçe Aktarıcı
- **Doku sorunları**: Doku setlerindeki doku yollarının Unity proje varlıklarına doğru şekilde eşlendiğinden emin olun
- **Yanlış arazi sırası**: Arazi sırasını manuel olarak ayarlamak için sürükle ve bırak arayüzünü kullanın
- **Eksik yükseklik haritaları**: Raw dosyalarının beklenen 6 basamaklı koordinat klasörlerinde olduğunu doğrulayın

#### Arazi Kenar Düzeltici
- **Görünür dikişler kalıyor**: Daha pürüzsüz geçişler için karıştırma mesafesi değerini artırın
- **Doku harmanlama sorunları**: Arazilerin uyumlu arazi katmanlarını paylaştığından emin olun

#### Katman Transfer Aracı
- **Transfer çalışmıyor**: Hem kaynak hem de hedef arazilerin geçerli TerrainData varlıklarına sahip olduğunu doğrulayın
- **Transferden sonra katmanlar görünmüyor**: Arazilerin uygun alfamap/splatmap atanmış olduğundan emin olun

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
